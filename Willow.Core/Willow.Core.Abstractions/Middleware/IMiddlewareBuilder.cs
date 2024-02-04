namespace Willow.Middleware;

/// <summary>
/// Builder for the <see cref="IMiddlewarePipeline{T}"/>.
/// </summary>
/// <typeparam name="T">Type of item that middleware process.</typeparam>
public interface IMiddlewareBuilder<T>
{
    /// <summary>
    /// Adds the middleware function to be called.
    /// </summary>
    /// <remarks>
    /// Adds a function to be called in the pipeline.
    /// </remarks>
    /// <param name="middleware">The middleware function.</param>
    public IMiddlewareBuilder<T> Add(Middleware<T> middleware);

    /// <summary>
    /// Adds a middleware to be used, this middleware uses the container to instantiate a new one every time the
    /// container is called.
    /// </summary>
    /// <remarks>
    /// Prefer <see cref="Add{TMiddleware}(TMiddleware)"/>, the reason is included in its documentation.
    /// </remarks>
    /// <typeparam name="TMiddleware">the type of middleware to use.</typeparam>
    public IMiddlewareBuilder<T> Add<TMiddleware>() where TMiddleware : IMiddleware<T>;

    /// <summary>
    /// Adds the middleware class to be called.
    /// </summary>
    /// <remarks>
    /// Since everything in the system is registered as a singleton, this is actually the preferred way to call this
    /// pipeline, the reason is that the consumer can request the pipeline items in the constructor and expose its
    /// dependencies better.
    /// </remarks>
    /// <typeparam name="TMiddleware">the type of middleware to use.</typeparam>
    /// <param name="middleware">The instantiated middleware.</param>
    public IMiddlewareBuilder<T> Add<TMiddleware>(TMiddleware middleware) where TMiddleware : IMiddleware<T>;

    public IMiddlewarePipeline<T> Build();
}
