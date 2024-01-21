using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Middleware.Abstractions;

namespace Willow.Core.Middleware;

internal sealed class MiddlewareBuilder<T> : IMiddlewareBuilder<T>
{
    private readonly List<Middleware<T>> _steps = [];
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MiddlewareBuilder<T>> _log;

    public MiddlewareBuilder(IServiceProvider serviceProvider, ILogger<MiddlewareBuilder<T>> log)
    {
        _serviceProvider = serviceProvider;
        _log = log;
    }

    public IMiddlewareBuilder<T> Add(Middleware<T> middleware)
    {
        _steps.Add(GetLoggedFunction(middleware));
        return this;
    }

    public IMiddlewareBuilder<T> Add<TMiddleware>(TMiddleware middleware) where TMiddleware : IMiddleware<T>
    {
        _steps.Add(GetLoggedFunction(middleware.ExecuteAsync));
        return this;
    }

    public IMiddlewareBuilder<T> Add<TMiddleware>() where TMiddleware : IMiddleware<T>
    {
        _steps.Add(GetLoggedFunction(ActualizedExecute));
        return this;

        async Task ActualizedExecute(T input, Func<T, Task> next)
        {
            await _serviceProvider.GetRequiredService<TMiddleware>().ExecuteAsync(input, next);
        }
    }

    public IMiddlewarePipeline<T> Build()
    {
        return new MiddlewarePipeline<T>(_steps);
    }

    private Middleware<T> GetLoggedFunction(Middleware<T> innerTask, string middlewareName = "anonymous")
    {
        return async (input, next) =>
        {
            _log.MiddlewareStarted(middlewareName);
            await innerTask(input, next);
            _log.MiddlewareEnded(middlewareName);
        };
    }
}

internal static partial class MiddlewareBuilderLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Trace, Message = "Started processing middleware ({middlewareName})")]
    public static partial void MiddlewareStarted(this ILogger logger, string middlewareName);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Ended processing middleware ({middlewareName})")]
    public static partial void MiddlewareEnded(this ILogger logger, string middlewareName);
}
