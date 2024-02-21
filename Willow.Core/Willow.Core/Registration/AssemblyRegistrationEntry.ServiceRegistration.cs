using System.Reflection;

using DryIoc;
using DryIoc.Microsoft.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using Willow.Helpers.Extensions;
using Willow.Registration.Exceptions;

namespace Willow.Registration;

internal sealed partial class AssemblyRegistrationEntry
{
    private ServiceRegistrationRecord[] RegisterServices(IEnumerable<IAssemblyRegistrar> registrars,
                                                         Assembly assembly,
                                                         bool registerAssemblyRegistrars = false)
    {
        var services = BuildServiceCollection(registrars, assembly, registerAssemblyRegistrars);
        EnsureAllSingletons(services);
        var preparedDescriptors = PrepareDescriptorsForProcessing(services);
        _log.RegisteringServices();
        var orderedRegistrations = RegisterServiceInOrderOfDependencies(preparedDescriptors);

        _log.ServicesFound(orderedRegistrations);

        orderedRegistrations.Reverse();
        return orderedRegistrations.ToArray();
    }

    private ServiceCollection BuildServiceCollection(IEnumerable<IAssemblyRegistrar> registrars,
                                                     Assembly assembly,
                                                     bool firstRun)
    {
        var services = new ServiceCollection();

        if (firstRun)
        {
            //We're running twice here, and registering a second time, because the registrar might depend on something
            //That is defined already within the system
            services.AddAllTypesDeriving<IAssemblyRegistrar>(assembly);
            CallServiceRegistrars(assembly, services);
        }

        RegisterExtensionTypes(registrars, assembly, services);


        return services;
    }

    private void RegisterExtensionTypes(IEnumerable<IAssemblyRegistrar> registrars,
                                        Assembly assembly,
                                        ServiceCollection services)
    {
        foreach (var type in registrars.SelectMany(static x => x.ExtensionTypes))
        {
            if (type.IsGenericTypeDefinition)
            {
                RegisterOpenGenericExtensionType(assembly, services, type);
            }
            else
            {
                RegisterExtensionType(assembly, services, type);
            }
        }
    }

    private void RegisterExtensionType(Assembly assembly, ServiceCollection services, Type type)
    {
        var types = assembly.GetAllDeriving(type);
        _log.TypesDetected(GetTypeNamesLogger(types));

        foreach (var concreteType in types)
        {
            services.AddSingleton(type, concreteType);
        }
    }

    private void RegisterOpenGenericExtensionType(Assembly assembly, ServiceCollection services, Type type)
    {
        var types = assembly.GetAllDerivingOpenGeneric(type);
        _log.OpenGenericTypesDetected(GetTypeNamesLogger(types));

        foreach (var concreteType in types)
        {
            var definition = concreteType.GetInterfaces()
                                         .Where(static x => x.IsGenericType)
                                         .First(x => x.GetGenericTypeDefinition() == type);

            services.AddSingleton(definition, concreteType);
        }
    }

    private static void CallServiceRegistrars(Assembly assembly, ServiceCollection services)
    {
        var serviceRegistrars = assembly.GetAllDeriving<IServiceRegistrar>();
        var activatedServiceRegistrar = serviceRegistrars.Select(Activator.CreateInstance).Cast<IServiceRegistrar>();
        foreach (var serviceRegistrar in activatedServiceRegistrar)
        {
            serviceRegistrar.RegisterServices(services);
        }
    }

    private static void EnsureAllSingletons(ServiceCollection services)
    {
        if (services.Any(static x => x.Lifetime is not ServiceLifetime.Singleton))
        {
            throw new RegistrationMustBeSingletonException();
        }
    }

    private static PreparedDescriptor[] PrepareDescriptorsForProcessing(ServiceCollection services)
    {
        var extendedServices = services.Select(GetServiceRegistrationAndCorrectDescriptor);
        var bundledDescriptors = extendedServices.Select(ToPreparedDescriptor).ToArray();
        return bundledDescriptors;

        static PreparedDescriptor ToPreparedDescriptor(DescriptorRecordPair x)
        {
            return new PreparedDescriptor(ServiceDescriptor: x,
                                          Dependencies: GetDependencies(x.RegistrationRecord.ImplementationType));
        }
    }

    private List<ServiceRegistrationRecord> RegisterServiceInOrderOfDependencies(PreparedDescriptor[] bundledDescriptors)
    {
        List<ServiceRegistrationRecord> orderedRegistrations = [];
        var iterations = 0;
        while (bundledDescriptors.Length != 0)
        {
            iterations++;
            var validServices = bundledDescriptors;

            //We want to make sure we are loading first the services that do not depend on services that are not registered
            //since the package registers them one by one, but honestly if we tried so long than its either missing dependency
            //which the package will report or the IsRegistered method of the container giving false negatives (which it
            //actually did do in testing) so just let it go.
            if (iterations < 10)
            {
                validServices = FilterServices(bundledDescriptors);
            }
            else
            {
                _log.IterationsExceeded(validServices);
            }

            _log.RegisteringIntoContainer(validServices);

            RegisterServiceInContainer(validServices);

            orderedRegistrations.AddRange(validServices.Select(static x => x.ServiceDescriptor.RegistrationRecord));
            bundledDescriptors = bundledDescriptors.Except(validServices).ToArray();
        }

        return orderedRegistrations;
    }

    private void RegisterServiceInContainer(PreparedDescriptor[] validServices)
    {
        foreach (var (descriptors, _) in validServices)
        {
            //When Services are removed, they somehow still stay inside in a way the IsRegistered returns false,
            //But when I do IfAlreadyRegistered.Keep, makes it not register, so there you go.
            if (!_container.IsRegistered(descriptors.Descriptor.ServiceType))
            {
                _container.RegisterDescriptor(descriptors.Descriptor, IfAlreadyRegistered.Replace);
            }

            if (descriptors.RegistrationRecord.ServiceType != descriptors.RegistrationRecord.ImplementationType)
            {
                _container.RegisterMapping(descriptors.RegistrationRecord.ServiceType,
                                           descriptors.RegistrationRecord.ImplementationType);

                //Some weird cache issue when I was calling the IEnumerable of a type, so just evict the cache,
                //honestly I wish there was a way to just dump the cache when we register...
                var enumeratorServiceType
                    = typeof(IEnumerable<>).MakeGenericType(descriptors.RegistrationRecord.ServiceType);
                _container.ClearCache(enumeratorServiceType);
            }
        }
    }

    private PreparedDescriptor[] FilterServices(PreparedDescriptor[] bundledDescriptors)
    {
        var validServices = bundledDescriptors.Where(AllDependenciesRegistered).ToArray();
        return validServices;

        bool AllDependenciesRegistered(PreparedDescriptor x)
        {
            return x.Dependencies.Any(types => Array.TrueForAll(types, IsRegistered));
        }
    }

    //This doesn't work very well, maybe it will be smarter to just check if the DI can just load the dependency
    private bool IsRegistered(Type type)
    {
        return _container.IsRegistered(type)
               || (type.IsGenericType && _container.IsRegistered(type.GetGenericTypeDefinition()));
    }

    private static Type[][] GetDependencies(Type t)
    {
        return t.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Select(static x => x.GetParameters().Select(static p => p.ParameterType).ToArray())
                .ToArray();
    }

    internal readonly record struct DescriptorRecordPair(ServiceDescriptor Descriptor,
                                                         ServiceRegistrationRecord RegistrationRecord);

    internal readonly record struct PreparedDescriptor(DescriptorRecordPair ServiceDescriptor, Type[][] Dependencies);
}
