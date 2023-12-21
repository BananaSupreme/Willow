namespace Willow.Core.Eventing.Abstractions;

public interface IEventDispatcher
{
    void Dispatch<TEvent>(TEvent @event)
        where TEvent : notnull;

    void RegisterHandler<TEvent, TEventHandler>()
        where TEventHandler : IEventHandler<TEvent>
        where TEvent : notnull;

    internal void RegisterInterceptor<TEvent, TInterceptor>()
        where TInterceptor : IEventInterceptor<TEvent>
        where TEvent : notnull;

    internal void RegisterGenericInterceptor<TGenericEventInterceptor>()
        where TGenericEventInterceptor : IGenericEventInterceptor;

    /// <summary>
    /// Flushes the tasks to ensure all tasks ran to completion, primarily there for tests, which is why this method is internal.
    /// </summary>
    /// <returns></returns>
    internal Task FlushAsync();
}