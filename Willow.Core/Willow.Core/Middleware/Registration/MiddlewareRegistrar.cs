using Microsoft.Extensions.DependencyInjection;

using Willow.Registration;

namespace Willow.Middleware.Registration;

internal sealed class MiddlewareRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(typeof(IMiddlewareBuilderFactory<>), typeof(MiddlewareBuilderFactory<>));
    }
}
