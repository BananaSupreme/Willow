namespace Willow.Core.Middleware.Abstractions;

//GUIDE_REQUIRED
/// <summary>
/// A middleware in the pipeline that processes input <typeparamref name="T"/>.
/// </summary>
/// <remarks>
/// The system generally discovers those automatically from registered assemblies and creates the implementor
/// using the DI container. <br/>
/// Middleware are registered as <i><b>singletons</b></i> within the system, so state cleanup is up to implementor.
/// </remarks>
/// <typeparam name="T">The the of input to process.</typeparam>
public interface IMiddleware<T>
{
    /// <summary>
    /// The execution function of the middleware.
    /// </summary>
    /// <param name="input">The input to process.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    public Task ExecuteAsync(T input, Func<T, Task> next);
}
