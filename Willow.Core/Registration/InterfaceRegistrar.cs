using System.Reflection;

using DryIoc;

using Willow.Core.Registration.Abstractions;
using Willow.Helpers.Extensions;
using Willow.Helpers.Logging.Loggers;

namespace Willow.Core.Registration;

internal sealed class InterfaceRegistrar : IInterfaceRegistrar
{
    private readonly ILogger<InterfaceRegistrar> _log;
    private readonly IRegistrator _registrator;

    public InterfaceRegistrar(IRegistrator registrator, ILogger<InterfaceRegistrar> log)
    {
        _registrator = registrator;
        _log = log;
    }

    public void RegisterDeriving<T>(Assembly[] assemblies)
    {
        RegisterDeriving(typeof(T), assemblies);
    }

    public void RegisterDeriving(Type typeToDeriveFrom, Assembly[] assemblies)
    {
        var types = assemblies.SelectMany(x => x.GetTypes()
                                                .Where(static type => !type.IsNestedPrivate)
                                                .Where(typeToDeriveFrom.IsAssignableFrom)
                                                .Where(static type => type.IsConcrete()))
                              .ToArray();

        _log.ImplementationsFound(new EnumeratorLogger<string>(types.Select(static x => x.Name)));
        foreach (var type in types)
        {
            _registrator.Register(type, new SingletonReuse(), ifAlreadyRegistered: IfAlreadyRegistered.Keep);
            _registrator.RegisterMapping(typeToDeriveFrom, type);
        }
    }
}

internal static partial class InterfaceRegistrarLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Located interfaces: {implementations}")]
    public static partial void ImplementationsFound(this ILogger logger, EnumeratorLogger<string> implementations);
}
