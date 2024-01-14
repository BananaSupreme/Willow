using System.Reflection;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.Registration.Abstractions;
using Willow.Helpers.Logging.Loggers;

namespace Willow.Core.Registration;

internal sealed class AssemblyRegistrationEntry : IAssemblyRegistrationEntry
{
    private readonly IEnumerable<IAssemblyRegistrar> _registrars;

    //We hold a special reference to this because it should be loaded first
    //in case any other registrar triggers events like registering commands
    private readonly IEventRegistrar _eventRegistrar;
    private readonly ILogger<AssemblyRegistrationEntry> _log;

    public AssemblyRegistrationEntry(IEnumerable<IAssemblyRegistrar> registrars,
                                     IEventRegistrar eventRegistrar,
                                     ILogger<AssemblyRegistrationEntry> log)
    {
        _registrars = registrars;
        _eventRegistrar = eventRegistrar;
        _log = log;
    }

    public void RegisterAssemblies(Assembly[] assemblies)
    {
        _log.ProcessingAssemblies(new(assemblies.Select(x => x.GetName().FullName)));
        _eventRegistrar.RegisterFromAssemblies(assemblies);
        foreach (var registrar in _registrars)
        {
            registrar.RegisterFromAssemblies(assemblies);
        }
    }
}

internal static partial class AssemblyRegistrationEntryLoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Processing assemblies: {assemblyNames}")]
    public static partial void ProcessingAssemblies(this ILogger logger, EnumeratorLogger<string> assemblyNames);
}