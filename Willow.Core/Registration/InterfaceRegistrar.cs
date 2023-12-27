using DryIoc;

using System.Reflection;

using Willow.Core.Helpers.Extensions;
using Willow.Core.Logging.Loggers;
using Willow.Core.Registration.Abstractions;

namespace Willow.Core.Registration;

internal class InterfaceRegistrar : IInterfaceRegistrar
{
    private readonly IRegistrator _registrator;
    private readonly ILogger<InterfaceRegistrar> _log;

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
        var types = assemblies.SelectMany(x =>
                                  x.GetTypes()
                                   .Where(type => !type.IsNestedPrivate)
                                   .Where(typeToDeriveFrom.IsAssignableFrom)
                                   .Where(type => type.IsConcrete()))
                              .ToArray();

        _log.ImplementationsFound(new(types.Select(x => x.Name)));
        foreach (var type in types)
        {
            _registrator.Register(typeToDeriveFrom, type, new SingletonReuse());
            _registrator.Register(type, new SingletonReuse());
        }
    }
}

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Located interfaces: {implementations}")]
    public static partial void ImplementationsFound(this ILogger logger, EnumeratorLogger<string> implementations);
}