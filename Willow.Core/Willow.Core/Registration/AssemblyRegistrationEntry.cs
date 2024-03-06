using System.Reflection;

using DryIoc;

using Willow.Helpers.Extensions;
using Willow.Helpers.Logging.Loggers;

using static Willow.Registration.AssemblyRegistrationEntry;

namespace Willow.Registration;

internal sealed partial class AssemblyRegistrationEntry : IAssemblyRegistrationEntry, IAsyncDisposable, IDisposable
{
    private readonly Dictionary<Guid, AssemblyRecord> _assemblyRecords = [];
    private readonly ILogger<AssemblyRegistrationEntry> _log;
    private readonly ICollectionProvider<IAssemblyRegistrar> _registrars;
    private readonly IContainer _container;
    private readonly IServiceProvider _serviceProvider;

    public AssemblyRegistrationEntry(ICollectionProvider<IAssemblyRegistrar> registrars,
                                     IContainer container,
                                     IServiceProvider serviceProvider,
                                     ILogger<AssemblyRegistrationEntry> log)
    {
        _registrars = registrars;
        _container = container;
        _serviceProvider = serviceProvider;
        _log = log;
    }

    public async Task<Guid[]> RegisterAssembliesAsync(Assembly[] assemblies)
    {
        return await assemblies.Select(RegisterAssemblyAsync).WhenAll();
    }

    private readonly record struct AssemblyRecord(Guid Id,
                                                  Assembly Assembly,
                                                  ServiceRegistrationRecord[] ServiceRegistrationRecord,
                                                  DateTime CreationTime);

    internal readonly record struct ServiceRegistrationRecord(Type ServiceType, Type ImplementationType);

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var (id, record) in _assemblyRecords.OrderBy(static x => x.Value.CreationTime))
        {
            if (_container.IsDisposed)
            {
                //If the container is disposed there is no point in unregistering, also it is an exception
                try
                {
                    StopServices(record);
                }
                catch (Exception)
                {
                    //We tried our best but it wasn't enough...
                }

                continue;
            }

            await UnregisterAssemblyAsync(id);
        }
    }

    private static EnumeratorLogger<string> GetTypeNamesLogger(Type[] types)
    {
        return new EnumeratorLogger<string>(types.Select(static x => x.ToString()));
    }
}

internal static partial class AssemblyRegistrationEntryLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Registering assembly: {assembly}")]
    public static partial void RegisteringAssembly(this ILogger logger, Assembly assembly);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Unregistering assembly: {assembly}")]
    public static partial void UnregisteringAssembly(this ILogger logger, Assembly assembly);

    [LoggerMessage(EventId = 3, Level = LogLevel.Trace, Message = "Created id from assembly ({id})")]
    public static partial void CreatedId(this ILogger logger, Guid id);

    [LoggerMessage(EventId = 4,
                   Level = LogLevel.Information,
                   Message
                       = "Created id from assembly (Id: {id}, Assembly: {assembly}, ServiceRegistrationRecords: {serviceRegistrationRecord})")]
    public static partial void CreatedRecord(this ILogger logger,
                                             Guid id,
                                             Assembly assembly,
                                             EnumeratorLogger<ServiceRegistrationRecord> serviceRegistrationRecord);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Error while trying to create the assembly record")]
    public static partial void FailedCreatingRecord(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "Registrars added from the assembly: ({registrars})")]
    public static partial void NewRegistrarsFromAssembly(this ILogger logger,
                                                         EnumeratorLogger<IAssemblyRegistrar> registrars);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Starting the services")]
    public static partial void StartingServices(this ILogger logger);

    [LoggerMessage(EventId = 8, Level = LogLevel.Error, Message = "Starting the services failed!")]
    public static partial void FailedStartingServices(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "Registering the services")]
    public static partial void RegisteringServices(this ILogger logger);

    [LoggerMessage(EventId = 10,
                   Level = LogLevel.Debug,
                   Message = "Registering the services into the container ({descriptors})")]
    public static partial void RegisteringIntoContainer(this ILogger logger,
                                                        EnumeratorLogger<PreparedDescriptor> descriptors);

    [LoggerMessage(EventId = 11, Level = LogLevel.Information, Message = "Found services in assembly: ({services})")]
    public static partial void ServicesFound(this ILogger logger, EnumeratorLogger<ServiceRegistrationRecord> services);

    [LoggerMessage(EventId = 12,
                   Level = LogLevel.Warning,
                   Message = "Number of iterations exceeded, registering whats left: ({descriptors})")]
    public static partial void IterationsExceeded(this ILogger logger, EnumeratorLogger<PreparedDescriptor> descriptors);

    [LoggerMessage(EventId = 13, Level = LogLevel.Error, Message = "Stopping the service failed!")]
    public static partial void FailedStoppingService(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 14, Level = LogLevel.Error, Message = "Tried to remove assembly that doesnt exist ({id})")]
    public static partial void AssemblyNotFound(this ILogger logger, Guid id);

    [LoggerMessage(EventId = 15, Level = LogLevel.Debug, Message = "Located open generic types: {types}")]
    public static partial void OpenGenericTypesDetected(this ILogger logger, EnumeratorLogger<string> types);

    [LoggerMessage(EventId = 16, Level = LogLevel.Debug, Message = "Located types: {types}")]
    public static partial void TypesDetected(this ILogger logger, EnumeratorLogger<string> types);
}
