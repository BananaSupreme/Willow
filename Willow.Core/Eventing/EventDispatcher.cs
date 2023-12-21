using Microsoft.Extensions.DependencyInjection;

using System.Threading.Channels;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.Helpers;
using Willow.Core.Helpers.Extensions;

namespace Willow.Core.Eventing;

internal sealed class EventDispatcher : IEventDispatcher, IUnsafeEventRegistrar
{
    private const string _genericInterceptorName = "Base";
    private readonly Dictionary<string, List<Type>> _eventHandlersStorage = [];
    private readonly Dictionary<string, List<Type>> _interceptorsStorage = [];
    private readonly Channel<Task> _runningTasks = Channel.CreateUnbounded<Task>();
    private readonly ILogger<EventDispatcher> _log;

    private readonly IServiceProvider _serviceProvider;

    public EventDispatcher(IServiceProvider serviceProvider, ILogger<EventDispatcher> log)
    {
        _serviceProvider = serviceProvider;
        _log = log;
        _ = RunQueue();
    }

    public void Dispatch<TEvent>(TEvent @event)
        where TEvent : notnull
    {
        var eventName = TypeExtensions.GetFullName<TEvent>();
        _log.EventDispatchStarting(eventName);
        if (!_eventHandlersStorage.TryGetValue(eventName, out var handlers))
        {
            _log.NoHandlerFound(eventName);
            return;
        }

        var actualizedHandlers = handlers.Select(Actualize<IEventHandler<TEvent>>).ToList();

        while (!_runningTasks.Writer.TryWrite(DispatchInternalAsync(@event, eventName, actualizedHandlers)))
        {
            //This item must be written before returning
        }
    }

    private async Task DispatchInternalAsync<TEvent>(TEvent @event, string eventName,
                                                     List<IEventHandler<TEvent>> actualizedHandlers)
        where TEvent : notnull
    {
        using var _ = _log.AddContext("eventType", eventName);
        var interceptors = GetInterceptors<TEvent>();
        if (interceptors.Count > 0)
        {
            _log.RunningWithInterceptors(interceptors.Count);
            var toRun = GetNextInterceptor(interceptors, actualizedHandlers, 0, _log);
            await toRun(@event);
        }
        else
        {
            _log.RunningWithoutInterceptors();
            await RunEvents(actualizedHandlers, @event, _log);
        }

        _log.EventDispatchCompleted();
    }

    public void RegisterHandler<TEvent, TEventHandler>()
        where TEventHandler : IEventHandler<TEvent>
        where TEvent : notnull
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

    public void RegisterInterceptor<TEvent, TInterceptor>()
        where TInterceptor : IEventInterceptor<TEvent>
        where TEvent : notnull
    {
        RegisterInterceptor(typeof(TEvent), typeof(TInterceptor));
    }

    public void RegisterInterceptor(Type eventType, Type interceptor)
    {
        var eventName = TypeExtensions.GetFullName(eventType);
        RegisterInterceptor(eventName, interceptor);
    }

    public void RegisterGenericInterceptor<TGenericEventInterceptor>()
        where TGenericEventInterceptor : IGenericEventInterceptor
    {
        RegisterGenericInterceptor(typeof(TGenericEventInterceptor));
    }

    public void RegisterGenericInterceptor(Type interceptor)
    {
        RegisterInterceptor(_genericInterceptorName, interceptor);
    }

    public async Task FlushAsync()
    {
        while (_runningTasks.Reader.TryRead(out var task))
        {
            await RunAndIgnoreErrorsAsync(task);
        }
    }

    private async Task RunQueue()
    {
        await foreach (var task in _runningTasks.Reader.ReadAllAsync())
        {
            await RunAndIgnoreErrorsAsync(task);
        }
    }

    private async Task RunAndIgnoreErrorsAsync(Task task)
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

    private static Func<TEvent, Task> GetNextInterceptor<TEvent>(List<InterceptorFunction<TEvent>> interceptors,
                                                                 List<IEventHandler<TEvent>> handlers,
                                                                 int currentIndex,
                                                                 ILogger<EventDispatcher> log)
        where TEvent : notnull
    {
        if (currentIndex < interceptors.Count - 1)
        {
            var nextInterceptor = interceptors[currentIndex];
            return @event => RunInterceptorLogged(nextInterceptor, @event,
                GetNextInterceptor(interceptors, handlers, currentIndex + 1, log), log);
        }

        var lastInterceptor = interceptors[^1];
        return @event =>
            RunInterceptorLogged(lastInterceptor, @event, newEvent => RunEvents(handlers, newEvent, log), log);
    }

    private static async Task RunInterceptorLogged<TEvent>(InterceptorFunction<TEvent> interceptor, TEvent @event,
                                                           Func<TEvent, Task> next, ILogger<EventDispatcher> log)
    {
        using var _ = log.AddContext("interceptorName", TypeExtensions.GetFullName(interceptor.GetType()));
        log.InterceptorExecutionStarting();
        await interceptor(@event, next);
    }

    private void RegisterInterceptor(string eventName, Type interceptorType)
    {
        _log.InterceptorRegistering(TypeExtensions.GetFullName(interceptorType), eventName);
        if (_interceptorsStorage.TryGetValue(eventName, out var interceptors))
        {
            interceptors.Add(interceptorType);
            return;
        }

        _interceptorsStorage.Add(eventName, [interceptorType]);
    }

    private List<InterceptorFunction<TEvent>> GetInterceptors<TEvent>()
    {
        List<InterceptorFunction<TEvent>> result = [];
        if (_interceptorsStorage.TryGetValue(_genericInterceptorName, out var genericInterceptors))
        {
            var actualized = genericInterceptors.Select(Actualize<IGenericEventInterceptor>).ToList();
            var toFunction =
                actualized.Select<IGenericEventInterceptor, InterceptorFunction<TEvent>>(x =>
                    async (@event, next) => await x.InterceptAsync(@event, next));
            result.AddRange(toFunction);
        }

        if (_interceptorsStorage.TryGetValue(TypeExtensions.GetFullName<TEvent>(), out var interceptors))
        {
            var actualized = interceptors.Select(Actualize<IEventInterceptor<TEvent>>).ToList();
            var toFunction =
                actualized.Select<IEventInterceptor<TEvent>, InterceptorFunction<TEvent>>(x =>
                    async (@event, next) => await x.InterceptAsync(@event, next));
            result.AddRange(toFunction);
        }

        return result;
    }

    private static async Task RunEvents<TEvent>(List<IEventHandler<TEvent>> handlers, TEvent @event, ILogger log)
        where TEvent : notnull
    {
        await SafeMultipleFunctionExecutor.ExecuteAsync(
            handlers,
            @event,
            async (handler, eventItem) =>
            {
                log.EventHandlerStarting();
                await handler.HandleAsync(eventItem);
            },
            (handler, _) =>
            {
                log.AddContext("handlerName", TypeExtensions.GetFullName(handler.GetType()));
            },
            (_, _, ex) =>
            {
                log.EventHandlingError(ex);
            });
    }

    private TResult Actualize<TResult>(Type value)
    {
        return (TResult)_serviceProvider.GetRequiredService(value);
    }

    private delegate Task InterceptorFunction<TEvent>(TEvent @event, Func<TEvent, Task> next);
}

internal static partial class EventDispatcherLoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "No handler found for event ({eventType}), aborting...")]
    public static partial void NoHandlerFound(this ILogger logger, string eventType);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Trace,
        Message = "Found ({handlerCount}) handlers for event.")]
    public static partial void HandlersFound(this ILogger logger, int handlerCount);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Trace,
        Message = "Dispatching event with ({interceptorCount}) interceptors.")]
    public static partial void RunningWithInterceptors(this ILogger logger, int interceptorCount);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Trace,
        Message = "Dispatching event without interceptors.")]
    public static partial void RunningWithoutInterceptors(this ILogger logger);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Debug,
        Message = "Registering handler of type ({handlerType}) for event ({eventType}).")]
    public static partial void HandlerRegistering(this ILogger logger, string handlerType, string eventType);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Debug,
        Message = "Registering interceptor of type ({interceptorType}) for event ({eventType}).")]
    public static partial void InterceptorRegistering(this ILogger logger, string interceptorType, string eventType);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Error,
        Message = "Error encountered while handling event:\r\n")]
    public static partial void EventHandlingError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Debug,
        Message = "Successfully handled event.")]
    public static partial void EventHandledSuccessfully(this ILogger logger);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Debug,
        Message = "Starting event handler.")]
    public static partial void EventHandlerStarting(this ILogger logger);

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Debug,
        Message = "Event dispatch completed.")]
    public static partial void EventDispatchCompleted(this ILogger logger);

    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Trace,
        Message = "Starting interceptor.")]
    public static partial void InterceptorExecutionStarting(this ILogger logger);

    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Debug,
        Message = "Starting event dispatch for event type ({eventType}).")]
    public static partial void EventDispatchStarting(this ILogger logger, string eventType);

    [LoggerMessage(
        EventId = 13,
        Level = LogLevel.Trace,
        Message = "Aggregated exceptions details.")]
    public static partial void AggregateExceptionEncountered(this ILogger logger, AggregateException ex);
}