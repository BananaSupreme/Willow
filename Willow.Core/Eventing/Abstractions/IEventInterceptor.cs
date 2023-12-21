namespace Willow.Core.Eventing.Abstractions;

internal interface IEventInterceptor<TEvent>
{
    Task InterceptAsync(TEvent @event, Func<TEvent, Task> next);
}