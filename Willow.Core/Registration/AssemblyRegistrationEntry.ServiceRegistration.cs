using System.Reflection;

using DryIoc;
using DryIoc.Microsoft.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Core.Registration.Exceptions;
using Willow.Helpers.Extensions;

namespace Willow.Core.Registration;

internal sealed partial class AssemblyRegistrationEntry
{
    private ServiceRegistrationRecord[] RegisterServices(IEnumerable<IAssemblyRegistrar> registrars,
                                                         Assembly assembly,
                                                         Guid assemblyId,
                                                         bool registerAssemblyRegistrars = false)
    {
        var services = BuildServiceCollection(registrars, assembly, assemblyId, registerAssemblyRegistrars);
        EnsureAllSingletons(services);
        var preparedDescriptors = PrepareDescriptorsForProcessing(services);
        _log.RegisteringServices();
        var orderedRegistrations = RegisterServiceInOrderOfDependencies(preparedDescriptors);

        _log.ServicesFound(orderedRegistrations);

        orderedRegistrations.Reverse();
        return orderedRegistrations.ToArray();
    }

    private static ServiceCollection BuildServiceCollection(IEnumerable<IAssemblyRegistrar> registrars,
                                                            Assembly assembly,
                                                            Guid assemblyId,
                                                            bool registerAssemblyRegistrars)
    {
        var services = new ServiceCollection();

        if (registerAssemblyRegistrars)
        {
            services.AddAllTypesDeriving<IAssemblyRegistrar>(assembly);
        }

        foreach (var registrar in registrars)
        {
            registrar.Register(assembly, assemblyId, services);
        }

        return services;
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
