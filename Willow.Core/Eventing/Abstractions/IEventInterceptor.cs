namespace Willow.Core.Eventing.Abstractions;

/// <summary>
/// Interface to implement interceptors in the system, make sure to read the remarks before resorting to this class.
/// </summary>
/// <list type="bullet">
/// <item>
/// The interceptor should also be registered with the DI container as it is instantiated using
/// the registered DI container, with all the dependencies that build upon it.
/// </item>
/// <item>
/// Interceptors are called in the order they are registered, which is why registering from a single file is
/// the recommended method of registration.<br/>
/// This makes them act effectively as middleware for various events, for now it is an implementation detail that
/// can change it is effective for certain types for events but can introduce difficulties if an handler doesn't
/// really expect them. For now it is mainly effective when reading audio data for preprocessing.
/// </item>
/// <item>
/// The chance of abuse here is quite high, so interceptors should be used sparingly and registration of
/// should be placed in a registration file next to the event.
/// <b>Eventually the design decision should be reconsidered!</b>
/// </item>
/// <item>
/// Besides all that all the issues raised above, all the considerations relevant for
/// <see cref="IEventHandler{TEvent}" />
/// are also valid here, such as thread-safety, no ability to register and registration as singleton.
/// </item>
/// </list>
/// <typeparam name="TEvent">The type of event to intercept.</typeparam>
internal interface IEventInterceptor<TEvent>
{
    /// <summary>
    /// The function that handles the interception.
    /// </summary>
    /// <param name="event">The incoming event item.</param>
    /// <param name="next">
    /// the next stage in the pipeline, if it is not called the chain to the events is interrupted in a fashion not unlike
    /// ASP.Net middleware.
    /// </param>
    Task InterceptAsync(TEvent @event, Func<TEvent, Task> next);
}
