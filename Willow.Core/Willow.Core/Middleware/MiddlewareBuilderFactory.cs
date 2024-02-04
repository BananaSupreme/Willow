namespace Willow.Middleware;

internal sealed class MiddlewareBuilderFactory<T> : IMiddlewareBuilderFactory<T>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;

    public MiddlewareBuilderFactory(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
    }

    public IMiddlewareBuilder<T> Create()
    {
        return new MiddlewareBuilder<T>(_serviceProvider, _loggerFactory.CreateLogger<MiddlewareBuilder<T>>());
    }
}
