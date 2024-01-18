using System.Reflection;

using DryIoc;

using Willow.Core.Eventing.Abstractions;
using Willow.Helpers.Extensions;
using Willow.Helpers.Logging.Loggers;

namespace Willow.Core.Eventing.Registration;

internal sealed class EventRegistrar : IEventRegistrar
{
    private readonly IEventDispatcher _eventDispatcher;
    private readonly ILogger<EventRegistrar> _log;
    private readonly IRegistrator _registrator;
    private readonly IUnsafeEventRegistrar _unsafeEventRegistrar;

    public EventRegistrar(IUnsafeEventRegistrar unsafeEventRegistrar,
                          IRegistrator registrator,
                          IEventDispatcher eventDispatcher,
                          ILogger<EventRegistrar> log)
    {
        _unsafeEventRegistrar = unsafeEventRegistrar;
        _registrator = registrator;
        _eventDispatcher = eventDispatcher;
        _log = log;
    }

    public void RegisterFromAssemblies(Assembly[] assemblies)
    {
        RegisterInterceptorsFromAssemblies(assemblies);
        RegisterInterceptors(assemblies);

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

    private void RegisterInterceptorsFromAssemblies(Assembly[] assemblies)
    {
        var types = assemblies.SelectMany(static assembly =>
                                              assembly.GetTypes()
                                                      .Where(static type => type.IsConcrete())
                                                      .Where(static type =>
                                                                 type.DerivesOpenGeneric(typeof(IEventInterceptor<>))))
                              .ToArray();

        _log.InterceptorsDetected(new EnumeratorLogger<string>(types.Select(static x => x.Name)));
        foreach (var type in types)
        {
            _registrator.Register(type, Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Keep);
        }
    }

    private void RegisterInterceptors(Assembly[] assemblies)
    {
        var registrars = assemblies.SelectMany(typeof(IInterceptorRegistrar).GetAllDerivingInAssembly)
                                   .Select(static registrar =>
                                               registrar.GetMethod(nameof(IInterceptorRegistrar.RegisterInterceptor)));
        foreach (var registrationMethod in registrars)
        {
            registrationMethod?.Invoke(null, [_eventDispatcher]);
        }
    }
}

internal static partial class EventRegistrarLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Located event handlers: {eventHandlerNames}")]
    public static partial void EventHandlersDetected(this ILogger logger, EnumeratorLogger<string> eventHandlerNames);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Located interceptors: {interceptorNames}")]
    public static partial void InterceptorsDetected(this ILogger logger, EnumeratorLogger<string> interceptorNames);
}
