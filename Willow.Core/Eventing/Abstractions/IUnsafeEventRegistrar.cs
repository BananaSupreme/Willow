namespace Willow.Core.Eventing.Abstractions;

/// <summary>
/// This is an unsafe registration point for events when the types are known in compile time.<br/>
/// <i><b>The implementation makes no effort to check that the types are reasonable and fit their strongly typed siblings
/// it is up to users of this interface to ensure type safety</b></i>.
/// </summary>
internal interface IUnsafeEventRegistrar
{
    /// <seealso cref="IEventDispatcher.RegisterInterceptor{TEvent,TInterceptor}"/>
    void RegisterInterceptor(Type eventType, Type interceptor);

    /// <seealso cref="IEventDispatcher.RegisterGenericInterceptor{TInterceptor}"/>
    void RegisterGenericInterceptor(Type interceptor);

    /// <seealso cref="IEventDispatcher.RegisterHandler{TEvent,TEventHandler}"/>
    void RegisterHandler(Type eventType, Type eventHandler);
}