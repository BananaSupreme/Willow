using DryIoc;

using Willow.Helpers.Extensions;
using Willow.Helpers.Logging.Extensions;
using Willow.Registration.Exceptions;

namespace Willow.Registration;

internal sealed partial class AssemblyRegistrationEntry
{
    public async Task UnregisterAssemblyAsync(Guid assemblyId)
    {
        var assemblyRecord = EnsureAssemblyRegistered(assemblyId);
        _log.UnregisteringAssembly(assemblyRecord.Assembly);
        _log.AddContext("assemblyName", assemblyRecord.Assembly.ToString());

        var typesToRemove = GetCleanedServiceRecords(assemblyRecord);
        try
        {
            StopServices(assemblyRecord);
            await DisposeServices(typesToRemove);
        }
        catch (Exception e)
        {
            throw new AssemblyLoadingException(e);
        }
        finally
        {
            UnregisterFromContainer(typesToRemove);
            _assemblyRecords.Remove(assemblyId);
        }
    }

    private void UnregisterFromContainer(ServiceRegistrationRecord[] typesToRemove)
    {
        foreach (var (serviceType, implementationType) in typesToRemove)
        {
            _container.Unregister(implementationType);
            _container.Unregister(serviceType, condition: factory => factory.ImplementationType == implementationType);
            _container.ClearCache(implementationType);
            _container.ClearCache(serviceType);
            _container.ClearCache(typeof(IEnumerable<>).MakeGenericType(serviceType));
        }
    }

    private async Task DisposeServices(ServiceRegistrationRecord[] typesToRemove)
    {
        foreach (var type in typesToRemove.Select(x => _container.Resolve(x.ImplementationType)))
        {
            switch (type)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }

    private void StopServices(AssemblyRecord assemblyRecord)
    {
        //Do we really care if the stop function excepted, more than we maybe want to log it?
        _ = _registrars.Get().Select(x => StopServiceAsync(x, assemblyRecord)).WhenAll();
    }

    private async Task StopServiceAsync(IAssemblyRegistrar assemblyRegistrar, AssemblyRecord assemblyRecord)
    {
        try
        {
            await assemblyRegistrar.StopAsync(assemblyRecord.Assembly,
                                              assemblyRecord.Id,
                                              new AssemblyBoundServiceProviderWrapper(
                                                  _serviceProvider,
                                                  assemblyRecord.Assembly));
        }
        catch (Exception e)
        {
            _log.FailedStoppingService(e);
        }
    }

    private static ServiceRegistrationRecord[] GetCleanedServiceRecords(AssemblyRecord assemblyRecord)
    {
        //Sometimes I got object out of the records for some reason, so just to make sure this doesn't happen
        return assemblyRecord.ServiceRegistrationRecord.Where(static x => x.ImplementationType != typeof(object))
                             .Distinct()
                             .ToArray();
    }

    private AssemblyRecord EnsureAssemblyRegistered(Guid assemblyId)
    {
        if (!_assemblyRecords.TryGetValue(assemblyId, out var assemblyRecord))
        {
            _log.AssemblyNotFound(assemblyId);
            throw new AssemblyNotFoundException();
        }

        return assemblyRecord;
    }
}
