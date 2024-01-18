using Willow.Core.Eventing.Abstractions;
using Willow.Helpers;
using Willow.Helpers.Extensions;
using Willow.Helpers.Logging.Extensions;

namespace Willow.Core.Eventing;

internal sealed partial class EventDispatcher
{
    public void Dispatch<TEvent>(TEvent @event) where TEvent : notnull
    {
        var eventName = TypeExtensions.GetFullName<TEvent>();
        _log.EventDispatchStarting(eventName);
        if (!_eventHandlersStorage.TryGetValue(eventName, out var handlers))
        {
            _log.NoHandlerFound(eventName);
            return;
        }

        var actualizedHandlers = handlers.Select(Actualize<IEventHandler<TEvent>>).ToList();

        //Only fails when channel marked completed
        _ = _runningTasks.Writer.TryWrite(DispatchInternalAsync(@event, eventName, actualizedHandlers));
    }

    private async Task DispatchInternalAsync<TEvent>(TEvent @event,
                                                     string eventName,
                                                     List<IEventHandler<TEvent>> actualizedHandlers)
        where TEvent : notnull
    {
        using var _ = _log.AddContext("eventType", eventName);
        var interceptors = GetInterceptors<TEvent>();
        if (interceptors.Count > 0)
        {
            _log.RunningWithInterceptors(interceptors.Count);
            var toRun = GetNextInterceptor(interceptors, actualizedHandlers, 0);
            await toRun(@event);
        }
        else
        {
            _log.RunningWithoutInterceptors();
            await RunEvents(actualizedHandlers, @event);
        }

        _log.EventDispatchCompleted();
    }

    private async Task RunEvents<TEvent>(List<IEventHandler<TEvent>> handlers, TEvent @event) where TEvent : notnull
    {
        var exceptions
            = await SafeMultipleFunctionExecutor.ExecuteAsync(handlers,
                                                              @event,
                                                              HandleEvent,
                                                              AddLogContext,
                                                              LogException);

        if (exceptions.Length > 0)
        {
            throw new AggregateException(exceptions);
        }
    }

    private async Task HandleEvent<TEvent>(IEventHandler<TEvent> handler, TEvent eventItem) where TEvent : notnull
    {
        _log.EventHandlerStarting();
        await handler.HandleAsync(eventItem);
    }

    private void AddLogContext<TEvent>(IEventHandler<TEvent> handler, TEvent _) where TEvent : notnull
    {
        _log.AddContext("handlerName", TypeExtensions.GetFullName(handler.GetType()));
    }

    private void LogException<TEvent>(IEventHandler<TEvent> _, TEvent __, Exception ex) where TEvent : notnull
    {
        _log.EventHandlingError(ex);
    }
}
