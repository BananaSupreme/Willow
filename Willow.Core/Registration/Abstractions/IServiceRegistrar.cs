using Microsoft.Extensions.DependencyInjection;

namespace Willow.Core.Registration.Abstractions;

public interface IServiceRegistrar
{
    static abstract void RegisterServices(IServiceCollection services);
}