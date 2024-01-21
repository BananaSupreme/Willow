using System.Reflection;

using DryIoc;

using Willow.Core.Eventing.Abstractions;
using Willow.Helpers.Extensions;
using Willow.Helpers.Logging.Loggers;

namespace Willow.Core.Eventing.Registration;

internal sealed class EventRegistrar : IEventRegistrar
{
    private readonly ILogger<EventRegistrar> _log;
    private readonly IRegistrator _registrator;
    private readonly IUnsafeEventRegistrar _unsafeEventRegistrar;

    public EventRegistrar(IUnsafeEventRegistrar unsafeEventRegistrar,
                          IRegistrator registrator,
                          ILogger<EventRegistrar> log)
    {
        _unsafeEventRegistrar = unsafeEventRegistrar;
        _registrator = registrator;
        _log = log;
    }

    public void RegisterFromAssemblies(Assembly[] assemblies)
    {
        var types = assemblies.SelectMany(static assembly =>
                                              assembly.GetTypes()
                                                      .Where(static type => type.IsConcrete())
                                                      .Where(static type =>
                                                                 type.DerivesOpenGeneric(typeof(IEventHandler<>))))
                              .ToArray();

        _log.EventHandlersDetected(new EnumeratorLogger<string>(types.Select(static x => x.Name)));
        foreach (var type in types)
        {
            var handlerType = type.GetInterfaces()
                                  .Where(static x => x.IsGenericType)
                                  .First(static x => x.GetGenericTypeDefinition() == typeof(IEventHandler<>));
            var eventType = handlerType.GenericTypeArguments[0];
            _registrator.Register(type, Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Keep);
            _unsafeEventRegistrar.RegisterHandler(eventType, type);
        }
    }
}

internal static partial class EventRegistrarLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Located event handlers: {eventHandlerNames}")]
    public static partial void EventHandlersDetected(this ILogger logger, EnumeratorLogger<string> eventHandlerNames);
}
