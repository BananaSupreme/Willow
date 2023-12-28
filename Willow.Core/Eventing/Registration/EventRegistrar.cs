using DryIoc;

using System.Reflection;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.Helpers.Extensions;
using Willow.Core.Logging.Loggers;

namespace Willow.Core.Eventing.Registration;

internal sealed class EventRegistrar : IEventRegistrar
{
    private readonly IUnsafeEventRegistrar _unsafeEventRegistrar;
    private readonly IRegistrator _registrator;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly ILogger<EventRegistrar> _log;

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

        var types = assemblies.SelectMany(assembly =>
                                  assembly.GetTypes()
                                          .Where(type => type.IsConcrete())
                                          .Where(type => type.DerivesOpenGeneric(typeof(IEventHandler<>))))
                              .ToArray();

        _log.EventHandlersDetected(new(types.Select(x => x.Name)));
        foreach (var type in types)
        {
            var handlerType = type.GetInterfaces().First(x => x.GetGenericTypeDefinition() == typeof(IEventHandler<>));
            var eventType = handlerType.GenericTypeArguments[0];

            _registrator.Register(type, Reuse.Singleton);
            _unsafeEventRegistrar.RegisterHandler(eventType, type);
        }
    }

    private void RegisterInterceptorsFromAssemblies(Assembly[] assemblies)
    {
        var types = assemblies.SelectMany(assembly =>
                                  assembly.GetTypes()
                                          .Where(type => type.IsConcrete())
                                          .Where(type => type.DerivesOpenGeneric(typeof(IEventInterceptor<>))))
                              .ToArray();

        _log.InterceptorsDetected(new(types.Select(x => x.Name)));
        foreach (var type in types)
        {
            _registrator.Register(type, Reuse.Singleton);
        }
    }

    private void RegisterInterceptors(Assembly[] assemblies)
    {
        var registrars = assemblies.SelectMany(typeof(IInterceptorRegistrar).GetAllDerivingInAssembly);
        foreach (var registrar in registrars)
        {
            var registrationMethod = registrar.GetMethod(nameof(IInterceptorRegistrar.RegisterInterceptor));
            registrationMethod?.Invoke(null, [_eventDispatcher]);
        }
    }
}

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Located event handlers: {eventHandlerNames}")]
    public static partial void EventHandlersDetected(this ILogger logger, EnumeratorLogger<string> eventHandlerNames);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "Located interceptors: {interceptorNames}")]
    public static partial void InterceptorsDetected(this ILogger logger, EnumeratorLogger<string> interceptorNames);
}