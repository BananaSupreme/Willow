using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Helpers.Extensions;
using Willow.Core.Registration.Abstractions;

namespace Willow.Core;

public static class WillowStartup
{
    public static void Register(IServiceCollection services, IConfiguration configuration)
    {
        RegisterServices(services);
        Configure(services, configuration);
    }
    
    private static void RegisterServices(IServiceCollection services)
    {
        var registrars = typeof(IServiceRegistrar).GetAllDerivingInOwnAssembly();
        foreach (var registrar in registrars)
        {
            var registrationMethod = registrar.GetMethod(nameof(IServiceRegistrar.RegisterServices));
            registrationMethod?.Invoke(null, [services]);
        }
    }
    
    private static void Configure(IServiceCollection services, IConfiguration configuration)
    {
        var registrars = typeof(IConfigurationRegistrar).GetAllDerivingInOwnAssembly();
        foreach (var registrar in registrars)
        {
            var registrationMethod = registrar.GetMethod(nameof(IConfigurationRegistrar.RegisterConfiguration));
            registrationMethod?.Invoke(null, [services, configuration]);
        }
    }

    
}