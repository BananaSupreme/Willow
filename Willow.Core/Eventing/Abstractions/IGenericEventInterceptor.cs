namespace Willow.Core.Eventing.Abstractions;

internal interface IGenericEventInterceptor
{
    Task InterceptAsync<TEvent>(TEvent @event, Func<TEvent, Task> next);
}