using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

using Willow.Core.Helpers.Extensions;
using Willow.Core.Registration.Abstractions;

namespace Willow.Core.Registration;

public static class WillowStartup
{
    public static void Register(Assembly[] assemblies, IServiceCollection services, IConfiguration configuration)
    {
        RegisterServices(assemblies, services);
        Configure(assemblies, services, configuration);
    }
    
    private static void RegisterServices(Assembly[] assemblies, IServiceCollection services)
    {
        var registrars = assemblies.SelectMany(assembly => typeof(IServiceRegistrar).GetAllDerivingInAssembly(assembly));
        foreach (var registrar in registrars)
        {
            var registrationMethod = registrar.GetMethod(nameof(IServiceRegistrar.RegisterServices));
            registrationMethod?.Invoke(null, [services]);
        }
    }
    
    private static void Configure(Assembly[] assemblies, IServiceCollection services, IConfiguration configuration)
    {
        var registrars = assemblies.SelectMany(assembly => typeof(IConfigurationRegistrar).GetAllDerivingInAssembly(assembly));
        foreach (var registrar in registrars)
        {
            var registrationMethod = registrar.GetMethod(nameof(IConfigurationRegistrar.RegisterConfiguration));
            registrationMethod?.Invoke(null, [services, configuration]);
        }
    }
}