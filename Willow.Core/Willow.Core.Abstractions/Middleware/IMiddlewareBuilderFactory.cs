namespace Willow.Middleware;

/// <summary>
/// Factory for a <see cref="IMiddlewareBuilder{T}"/>.
/// </summary>
/// <typeparam name="T">Type of input to process.</typeparam>
public interface IMiddlewareBuilderFactory<T>
{
    /// <summary>
    /// Create a new instance of <see cref="IMiddlewareBuilder{T}"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="IMiddlewareBuilder{T}"/>.</returns>
    public IMiddlewareBuilder<T> Create();
}
