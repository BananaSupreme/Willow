using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Willow.Core.Middleware.Abstractions;
using Willow.Core.Registration.Abstractions;
using Willow.Helpers.Extensions;
using Willow.Helpers.Logging.Loggers;

namespace Willow.Core.Middleware.Registration;

/// <summary>
/// Registers all the <see cref="IMiddleware{T}"/> in the assemblies.
/// </summary>
internal sealed class MiddlewareAssemblyRegistrar : IAssemblyRegistrar
{
    private readonly ILogger<MiddlewareAssemblyRegistrar> _log;

    public MiddlewareAssemblyRegistrar(ILogger<MiddlewareAssemblyRegistrar> log)
    {
        _log = log;
    }

    public void Register(Assembly assembly, Guid assemblyId, IServiceCollection services)
    {
        var types = assembly.GetAllDerivingOpenGeneric(typeof(IMiddleware<>));

        _log.MiddlewareDetected(new EnumeratorLogger<string>(types.Select(static x => x.Name)));
        foreach (var type in types)
        {
            services.TryAddSingleton(type);
        }
    }

    public Task StartAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        return Task.CompletedTask;
    }
}

internal static partial class MiddlewareAssemblyRegistrarLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Located middleware: {eventHandlerNames}")]
    public static partial void MiddlewareDetected(this ILogger logger, EnumeratorLogger<string> eventHandlerNames);
}
