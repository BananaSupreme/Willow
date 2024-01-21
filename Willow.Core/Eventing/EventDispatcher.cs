using System.Threading.Channels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Willow.Core.Eventing.Abstractions;
using Willow.Helpers.Extensions;

namespace Willow.Core.Eventing;

internal sealed partial class EventDispatcher : BackgroundService, IEventDispatcher, IUnsafeEventRegistrar
{
    private readonly Dictionary<string, List<Type>> _eventHandlersStorage = [];
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

    public void RegisterHandler(Type eventType, Type eventHandler)
    {
        var eventName = TypeExtensions.GetFullName(eventType);
        _log.HandlerRegistering(TypeExtensions.GetFullName(eventHandler), eventName);
        if (_eventHandlersStorage.TryGetValue(eventName, out var handlers))
        {
            handlers.Add(eventHandler);
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(async () => await RunQueue(stoppingToken), stoppingToken);
    }

    private async Task RunQueue(CancellationToken stoppingToken)
    {
        await foreach (var task in _runningTasks.Reader.ReadAllAsync(stoppingToken))
        {
            await RunAndIgnoreErrorsAsync(task);
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

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Error encountered while handling event:\r\n")]
    public static partial void EventHandlingError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "Successfully handled event.")]
    public static partial void EventHandledSuccessfully(this ILogger logger);

    [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "Starting event handler.")]
    public static partial void EventHandlerStarting(this ILogger logger);

    [LoggerMessage(EventId = 8, Level = LogLevel.Debug, Message = "Event dispatch completed.")]
    public static partial void EventDispatchCompleted(this ILogger logger);

    [LoggerMessage(EventId = 9,
                   Level = LogLevel.Debug,
                   Message = "Starting event dispatch for event type ({eventType}).")]
    public static partial void EventDispatchStarting(this ILogger logger, string eventType);

    [LoggerMessage(EventId = 10, Level = LogLevel.Trace, Message = "Aggregated exceptions details.")]
    public static partial void AggregateExceptionEncountered(this ILogger logger, AggregateException ex);
}
