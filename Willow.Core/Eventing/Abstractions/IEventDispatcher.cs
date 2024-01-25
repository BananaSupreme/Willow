namespace Willow.Core.Eventing.Abstractions;

//GUIDE_REQUIRED EXPLAIN ABOUT EVENTING
/// <summary>
/// Responsible for dispatching and managing events in the system.
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatch an event to the system to be processed by all registered handlers.
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
    /// the registered DI container, with all the dependencies that build upon it.
    /// </remarks>
    /// <seealso cref="IEventHandler{TEvent}" />
    /// <typeparam name="TEvent">Type of event the handler must handle.</typeparam>
    /// <typeparam name="TEventHandler">Type of handler.</typeparam>
    void RegisterHandler<TEvent, TEventHandler>() where TEventHandler : IEventHandler<TEvent> where TEvent : notnull;

    /// <summary>
    /// Registers an event handler for <typeparamref name="TEvent" />.<br/>
    /// </summary>
    /// <remarks>
    /// Generally speaking, handlers are unregistered automatically with the system when the assembly unloads.
    /// <b>If it is not</b> and this function is called for some reason
    /// the handler should also be unregistered with the DI container.
    /// </remarks>
    /// <seealso cref="IEventHandler{TEvent}" />
    /// <typeparam name="TEvent">Type of event the handler must handle.</typeparam>
    /// <typeparam name="TEventHandler">Type of handler.</typeparam>
    void UnregisterHandler<TEvent, TEventHandler>() where TEventHandler : IEventHandler<TEvent> where TEvent : notnull;

    /// <summary>
    /// Flushes the tasks to ensure all tasks ran to completion, primarily there for tests, which is why this method is
    /// internal.
    /// </summary>
    internal void Flush();
}
