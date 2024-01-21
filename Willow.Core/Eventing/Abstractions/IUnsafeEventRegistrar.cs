namespace Willow.Core.Eventing.Abstractions;

/// <summary>
/// This is an unsafe registration point for events when the types are known in compile time.<br/>
/// <b><i>The implementation makes no effort to check that the types are reasonable and fit their strongly typed
/// siblings it is up to users of this interface to ensure type safety</i></b>.
/// </summary>
internal interface IUnsafeEventRegistrar
{
    /// <seealso cref="IEventDispatcher.RegisterHandler{TEvent,TEventHandler}" />
    void RegisterHandler(Type eventType, Type eventHandler);
}
