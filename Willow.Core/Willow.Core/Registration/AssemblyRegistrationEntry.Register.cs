using System.Reflection;

using Willow.Helpers.Extensions;
using Willow.Helpers.Logging.Extensions;
using Willow.Helpers.Logging.Loggers;
using Willow.Registration.Exceptions;

namespace Willow.Registration;

internal sealed partial class AssemblyRegistrationEntry
{
    public async Task<Guid> RegisterAssemblyAsync(Assembly assembly)
    {
        _log.RegisteringAssembly(assembly);
        using var _ = _log.AddContext("assemblyName", assembly.ToString());

        var assemblyId = Guid.NewGuid();

        _log.CreatedId(assemblyId);
        CreateAssemblyRecord(assembly, assemblyId);

        await StartServicesAsync(assembly, assemblyId);

        return assemblyId;
    }

    private void CreateAssemblyRecord(Assembly assembly, Guid assemblyId)
    {
        try
        {
            if (_assemblyRecords.Any(x => x.Value.Assembly == assembly))
            {
                throw new AssemblyAlreadyLoadedException();
            }

            var registrars = _registrars.Get().ToArray();
            var orderedServices = RegisterServices(registrars, assembly, assemblyId, true);

            //We want to make sure we also register the assembly with the Registrars defined within it.
            orderedServices = RegisterFromNewRegistrars(assembly, assemblyId, registrars, orderedServices);
            var record = new AssemblyRecord(assemblyId, assembly, orderedServices);

            _log.CreatedRecord(record.Id, record.Assembly, record.ServiceRegistrationRecord);

            _assemblyRecords.Add(assemblyId, record);
        }
        catch (Exception e)
        {
            _log.FailedCreatingRecord(e);
            throw new AssemblyLoadingException(e);
        }
    }

    private ServiceRegistrationRecord[] RegisterFromNewRegistrars(Assembly assembly,
                                                                  Guid assemblyId,
                                                                  IAssemblyRegistrar[] registrars,
                                                                  ServiceRegistrationRecord[] orderedServices)
    {
        var extraRegistrars = _registrars.Get().Except(registrars).ToArray();
        if (extraRegistrars.Length != 0)
        {
            _log.NewRegistrarsFromAssembly(new EnumeratorLogger<IAssemblyRegistrar>(extraRegistrars));
            orderedServices = orderedServices.Union(RegisterServices(extraRegistrars, assembly, assemblyId)).ToArray();
        }

        return orderedServices;
    }

    private async Task StartServicesAsync(Assembly assembly, Guid assemblyId)
    {
        try
        {
            _log.StartingServices();
            await _registrars.Get().Select(StartServiceAsync).WhenAll();
        }
        catch (Exception e)
        {
            _log.FailedStartingServices(e);
            await UnregisterAssemblyAsync(assemblyId);
            throw new AssemblyLoadingException(e);
        }

        Task StartServiceAsync(IAssemblyRegistrar x)
        {
            return x.StartAsync(assembly,
                                assemblyId,
                                new AssemblyBoundServiceProviderWrapper(_serviceProvider, assembly));
        }
    }
}
