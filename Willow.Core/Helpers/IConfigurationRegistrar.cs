using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Willow.Core.Helpers;

public interface IConfigurationRegistrar
{
    static abstract void RegisterConfiguration(IServiceCollection services, IConfiguration configuration);
}