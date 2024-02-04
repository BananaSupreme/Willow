using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Willow.Eventing.Abstractions;
using Willow.Helpers.Extensions;
using Willow.Helpers.Logging.Loggers;
using Willow.Registration;

namespace Willow.Eventing.Registration;

internal sealed class EventingAssemblyRegistrar : IAssemblyRegistrar
{
    private readonly ILogger<EventingAssemblyRegistrar> _log;
    private IUnsafeEventRegistrar? _unsafeEventRegistrar;

    public EventingAssemblyRegistrar(ILogger<EventingAssemblyRegistrar> log)
    {
        _log = log;
    }

    public EventingAssemblyRegistrar(IUnsafeEventRegistrar unsafeEventRegistrar, ILogger<EventingAssemblyRegistrar> log)
    {
        _unsafeEventRegistrar = unsafeEventRegistrar;
        _log = log;
    }

    public void Register(Assembly assembly, Guid assemblyId, IServiceCollection services)
    {
        var types = assembly.GetAllDerivingOpenGeneric(typeof(IEventHandler<>));
        _log.EventHandlersDetected(GetTypeNamesLogger(types));

        foreach (var type in types)
        {
            services.TryAddSingleton(type);
        }
    }

    public Task StartAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        _unsafeEventRegistrar ??= serviceProvider.GetRequiredService<IUnsafeEventRegistrar>();

        var types = assembly.GetAllDerivingOpenGeneric(typeof(IEventHandler<>));
        _log.EventHandlersDetected(GetTypeNamesLogger(types));

        foreach (var type in types)
        {
            foreach (var eventType in GetEventType(type))
            {
                _unsafeEventRegistrar.RegisterHandler(eventType, type);
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        _unsafeEventRegistrar ??= serviceProvider.GetRequiredService<IUnsafeEventRegistrar>();

        var types = assembly.GetAllDerivingOpenGeneric(typeof(IEventHandler<>));
        _log.EventHandlersDetected(GetTypeNamesLogger(types));

        foreach (var type in types)
        {
            foreach (var eventType in GetEventType(type))
            {
                _unsafeEventRegistrar.UnregisterHandler(eventType, type);
            }
        }

        return Task.CompletedTask;
    }

    private static IEnumerable<Type> GetEventType(Type type)
    {
        return type.GetInterfaces()
                   .Where(static x => x.IsGenericType)
                   .Where(static x => x.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                   .Select(static x => x.GenericTypeArguments[0]);
    }

    private static EnumeratorLogger<string> GetTypeNamesLogger(Type[] types)
    {
        return new EnumeratorLogger<string>(types.Select(static x => x.ToString()));
    }
}

internal static partial class EventRegistrarLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Located event handlers: {eventHandlerNames}")]
    public static partial void EventHandlersDetected(this ILogger logger, EnumeratorLogger<string> eventHandlerNames);
}
