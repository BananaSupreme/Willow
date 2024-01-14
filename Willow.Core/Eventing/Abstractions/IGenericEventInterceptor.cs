namespace Willow.Core.Eventing.Abstractions;

/// <summary>
/// Interface to implement generic interceptors for cross cutting concerns.
/// </summary>
/// <remarks>
/// There are a few issues with interceptor, only now applicable to every event and every handler.<br/>
/// For a more detailed description of concerns raised with interceptors consider <see cref="IEventInterceptor{TEvent}"/>.
/// </remarks>
internal interface IGenericEventInterceptor
{
    /// <summary>
    /// The interception handler.
    /// </summary>
    /// <param name="event">
    /// The event object incoming, consider that we do not know the type the incoming event the purpose of this class
    /// is to handle cross cutting concern, rather than anything specific.
    /// </param>
    /// <param name="next">
    /// the next stage in the pipeline, if it is not called the chain to the events is interrupted in a fashion not unlike
    /// ASP.Net middleware.
    /// </param>
    /// <typeparam name="TEvent">The type of event that was called.</typeparam>
    Task InterceptAsync<TEvent>(TEvent @event, Func<TEvent, Task> next);
}