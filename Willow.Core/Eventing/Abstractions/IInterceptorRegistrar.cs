namespace Willow.Core.Eventing.Abstractions;

/// <summary>
/// A registration point for interceptors, discovered automatically by the assembly scanning ran at startup
/// Each event should only have one of those, here is where the interceptors should be organized so the ordering of
/// them is deterministic.
/// </summary>
internal interface IInterceptorRegistrar
{
    /// <summary>
    /// Registration function for interceptors.
    /// </summary>
    /// <param name="eventDispatcher">A dispatcher to be registered onto.</param>
    static abstract void RegisterInterceptor(IEventDispatcher eventDispatcher);
}
