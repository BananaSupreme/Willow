using System.Threading.Channels;

using Microsoft.Extensions.DependencyInjection;

using Willow.Eventing.Abstractions;
using Willow.Registration;

namespace Willow.Eventing;

internal sealed partial class EventDispatcher : IBackgroundWorker, IEventDispatcher, IUnsafeEventRegistrar
{
    private readonly Dictionary<string, HashSet<Type>> _eventHandlersStorage = [];
    private readonly ILogger<EventDispatcher> _log;
    private readonly Channel<Task> _runningTasks = Channel.CreateUnbounded<Task>();

    private readonly IServiceProvider _serviceProvider;

    public EventDispatcher(IServiceProvider serviceProvider, ILogger<EventDispatcher> log)
    {
        _serviceProvider = serviceProvider;
        _log = log;
    }

    public void RegisterHandler<TEvent, TEventHandler>()
        where TEventHandler : IEventHandler<TEvent> where TEvent : notnull
    {
        RegisterHandler(typeof(TEvent), typeof(TEventHandler));
    }

    public void UnregisterHandler<TEvent, TEventHandler>()
        where TEventHandler : IEventHandler<TEvent> where TEvent : notnull
    {
        UnregisterHandler(typeof(TEvent), typeof(TEventHandler));
    }

    public void RegisterHandler(Type eventType, Type eventHandler)
    {
        var eventName = eventType.ToString();
        _log.HandlerRegistering(eventHandler.ToString(), eventName);
        if (_eventHandlersStorage.TryGetValue(eventName, out var handlers))
        {
            handlers.Add(eventHandler);
            return;
        }

        _eventHandlersStorage.Add(eventName, [eventHandler]);
    }

    public void UnregisterHandler(Type eventType, Type eventHandler)
    {
        var eventName = eventType.ToString();
        _log.HandlerUnregistering(eventHandler.ToString(), eventName);
        if (_eventHandlersStorage.TryGetValue(eventName, out var handlers))
        {
            handlers.Remove(eventHandler);
            return;
        }

        _eventHandlersStorage.Add(eventName, [eventHandler]);
    }

    public void Flush()
    {
        while (_runningTasks.Reader.TryRead(out var task))
        {
            RunAndIgnoreErrorsAsync(task).GetAwaiter().GetResult();
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await RunQueue(cancellationToken);
    }

    public Task StopAsync()
    {
        Flush();
        return Task.CompletedTask;
    }

    private async Task RunQueue(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var task in _runningTasks.Reader.ReadAllAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await RunAndIgnoreErrorsAsync(task);
            }
        }
        catch (OperationCanceledException)
        {
            //nothing to do here
        }
    }

    private static async Task RunAndIgnoreErrorsAsync(Task task)
    {
        try
        {
            await task;
        }
        catch
        {
            //ignored
        }
    }

    private TResult Actualize<TResult>(Type value)
    {
        return (TResult)_serviceProvider.GetRequiredService(value);
    }
}

internal static partial class EventDispatcherLoggingExtensions
{
    [LoggerMessage(EventId = 1,
                   Level = LogLevel.Information,
                   Message = "No handler found for event ({eventType}), aborting...")]
    public static partial void NoHandlerFound(this ILogger logger, string eventType);

    [LoggerMessage(EventId = 2, Level = LogLevel.Trace, Message = "Found ({handlerCount}) handlers for event.")]
    public static partial void HandlersFound(this ILogger logger, int handlerCount);

    [LoggerMessage(EventId = 3, Level = LogLevel.Trace, Message = "Dispatching event.")]
    public static partial void RunningEvent(this ILogger logger);

    [LoggerMessage(EventId = 4,
                   Level = LogLevel.Debug,
                   Message = "Registering handler of type ({handlerType}) for event ({eventType}).")]
    public static partial void HandlerRegistering(this ILogger logger, string handlerType, string eventType);

    [LoggerMessage(EventId = 5,
                   Level = LogLevel.Debug,
                   Message = "Unregistering handler of type ({handlerType}) for event ({eventType}).")]
    public static partial void HandlerUnregistering(this ILogger logger, string handlerType, string eventType);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Error encountered while handling event:\r\n")]
    public static partial void EventHandlingError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "Successfully handled event.")]
    public static partial void EventHandledSuccessfully(this ILogger logger);

    [LoggerMessage(EventId = 8, Level = LogLevel.Debug, Message = "Starting event handler.")]
    public static partial void EventHandlerStarting(this ILogger logger);

    [LoggerMessage(EventId = 9, Level = LogLevel.Debug, Message = "Event dispatch completed.")]
    public static partial void EventDispatchCompleted(this ILogger logger);

    [LoggerMessage(EventId = 10,
                   Level = LogLevel.Debug,
                   Message = "Starting event dispatch for event type ({eventType}).")]
    public static partial void EventDispatchStarting(this ILogger logger, string eventType);

    [LoggerMessage(EventId = 11, Level = LogLevel.Trace, Message = "Aggregated exceptions details.")]
    public static partial void AggregateExceptionEncountered(this ILogger logger, AggregateException ex);
}
