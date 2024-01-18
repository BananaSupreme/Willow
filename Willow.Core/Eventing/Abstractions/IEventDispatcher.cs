namespace Willow.Core.Eventing.Abstractions;

/// <summary>
/// Responsible for dispatching and managing events in the system.
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatch an event to the system, the event would be called asynchronously by all handlers if applicable
    /// after being processed by all the relevant interceptors.
    /// </summary>
    /// <param name="event">Event parameters.</param>
    /// <typeparam name="TEvent">Type of event to process.</typeparam>
    void Dispatch<TEvent>(TEvent @event) where TEvent : notnull;

    /// <summary>
    /// Registers an event handler for <typeparamref name="TEvent" />.<br/>
    /// </summary>
    /// <remarks>
    /// Generally speaking, handlers are registered automatically with the system through assembly scanning.
    /// <b>If it is not</b> and this function is called for some reason
    /// the handler should also be registered with the DI container as it is instantiated using
    /// the registered DI container, with all the dependencies that build upon it.<br/>
    /// </remarks>
    /// <seealso cref="IEventHandler{TEvent}" />
    /// <typeparam name="TEvent">Type of event the handler must handle.</typeparam>
    /// <typeparam name="TEventHandler">Type of handler.</typeparam>
    void RegisterHandler<TEvent, TEventHandler>() where TEventHandler : IEventHandler<TEvent> where TEvent : notnull;
    /// <summary>
    /// An interceptor to run between the event and the handlers registered to it. <br/>
    /// </summary>
    /// <remarks>
    /// Interceptor usage should be done sparingly and question,
    /// for more in-depth discussion check out the see also section.
    /// </remarks>
    /// <seealso cref="IEventInterceptor{TEvent}" />
    /// <typeparam name="TEvent">The type of the event the interceptor should intercept.</typeparam>
    /// <typeparam name="TInterceptor">The type of the interceptor.</typeparam>
    internal void RegisterInterceptor<TEvent, TInterceptor>()
        where TInterceptor : IEventInterceptor<TEvent> where TEvent : notnull;

    /// <summary>
    /// An interceptor to run between all events and all handlers registered. <br/>
    /// </summary>
    /// <remarks>
    /// Same issues as regular interceptors, now they apply to every single event.
    /// <b><i>check out the see also section</i></b>.<br/>
    /// The generic ones were originally created for cross-cutting concerns, for now it remains unused.
    /// </remarks>
    /// <seealso cref="RegisterInterceptor{TEvent,TInterceptor}" />
    /// <seealso cref="IEventInterceptor{TEvent}" />
    /// <seealso cref="IGenericEventInterceptor" />
    /// <typeparam name="TGenericEventInterceptor">Type of generic interceptor.</typeparam>
    internal void RegisterGenericInterceptor<TGenericEventInterceptor>()
        where TGenericEventInterceptor : IGenericEventInterceptor;

    /// <summary>
    /// Flushes the tasks to ensure all tasks ran to completion, primarily there for tests, which is why this method is
    /// internal.
    /// </summary>
    internal void Flush();
}
