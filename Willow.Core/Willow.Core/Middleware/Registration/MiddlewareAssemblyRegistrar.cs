using System.Reflection;

using Willow.Registration;

namespace Willow.Middleware.Registration;

/// <summary>
/// Registers all the <see cref="IMiddleware{T}"/> in the assemblies.
/// </summary>
internal sealed class MiddlewareAssemblyRegistrar : IAssemblyRegistrar
{
    public Type[] ExtensionTypes => [typeof(IMiddleware<>)];

    public Task StartAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        return Task.CompletedTask;
    }
}
