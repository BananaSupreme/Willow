using System.Reflection;

using DryIoc;

using Willow.Core.Middleware.Abstractions;
using Willow.Core.Registration.Abstractions;
using Willow.Helpers.Extensions;
using Willow.Helpers.Logging.Loggers;

namespace Willow.Core.Middleware.Registration;

/// <summary>
/// Registers all the <see cref="IMiddleware{T}"/> in the assemblies.
/// </summary>
internal sealed class MiddlewareAssemblyRegistrar : IAssemblyRegistrar
{
    private readonly ILogger<MiddlewareAssemblyRegistrar> _log;
    private readonly IRegistrator _registrator;

    public MiddlewareAssemblyRegistrar(ILogger<MiddlewareAssemblyRegistrar> log, IRegistrator registrator)
    {
        _log = log;
        _registrator = registrator;
    }

    public void RegisterFromAssemblies(Assembly[] assemblies)
    {
        var types = assemblies.SelectMany(static assembly =>
                                              assembly.GetTypes()
                                                      .Where(static type => type.IsConcrete())
                                                      .Where(static type =>
                                                                 type.DerivesOpenGeneric(typeof(IMiddleware<>))))
                              .ToArray();

        _log.MiddlewareDetected(new EnumeratorLogger<string>(types.Select(static x => x.Name)));
        foreach (var type in types)
        {
            _registrator.Register(type, Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Keep);
        }
    }
}

internal static partial class MiddlewareAssemblyRegistrarLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Located middleware: {eventHandlerNames}")]
    public static partial void MiddlewareDetected(this ILogger logger, EnumeratorLogger<string> eventHandlerNames);
}
