using Microsoft.Extensions.DependencyInjection;

namespace Willow.Core.Helpers;

public interface IServiceRegistrar
{
    static abstract void RegisterServices(IServiceCollection services);
}