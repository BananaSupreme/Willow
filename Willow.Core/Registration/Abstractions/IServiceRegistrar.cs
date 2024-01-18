using Microsoft.Extensions.DependencyInjection;

namespace Willow.Core.Registration.Abstractions;

/// <summary>
/// Registration point for modules to register with the DI.
/// </summary>
/// <remarks>
/// Called before the construction of the container.
/// </remarks>
public interface IServiceRegistrar
{
    /// <inheritdoc cref="IServiceRegistrar" />
    static abstract void RegisterServices(IServiceCollection services);
}
