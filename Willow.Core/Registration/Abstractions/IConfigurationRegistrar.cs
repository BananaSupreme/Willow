using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Willow.Core.Registration.Abstractions;

public interface IConfigurationRegistrar
{
    static abstract void RegisterConfiguration(IServiceCollection services, IConfiguration configuration);
}