namespace Willow.Core.Eventing.Abstractions;

/// <summary>
/// An event handler for the incoming event.<br/>
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>
/// The system generally discovers those automatically from registered assemblies and creates the implementor
/// using the DI container.
/// </item>
/// <item>
/// Events are distributed <i><b>asynchronously</b></i> without any locking mechanism, handlers should ensure
/// they are <i><b>thread-safe</b></i>.
/// </item>
/// <item>
/// Events are registered as <i><b>singletons</b></i> within the system, so state cleanup is up to handler.
/// </item>
/// <item>
/// There is no way to <i><b>remove</b></i> an event at this moment, so event should control whether they
/// should run or not.
/// </item>
/// </list>
/// </remarks>
/// <typeparam name="TEvent">Type of event to handle.</typeparam>
public interface IEventHandler<in TEvent>
{
    /// <summary>
    /// The handler for the event.
    /// </summary>
    /// <param name="event">The event item.</param>
    Task HandleAsync(TEvent @event);
}
