namespace Willow.Middleware;

/// <summary>
/// A generic interface for a middleware pipeline.
/// </summary>
/// <typeparam name="T">The type of the input for the middleware pipeline.</typeparam>
public interface IMiddlewarePipeline<T>
{
    /// <summary>
    /// Executes the middleware pipeline.
    /// </summary>
    /// <param name="input">The input to be processed by the middleware.</param>
    /// <param name="end">The final handler to be executed at the end of the pipeline.</param>
    public Task ExecuteAsync(T input, Handler<T> end);
}
