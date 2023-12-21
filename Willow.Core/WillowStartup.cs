using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.Helpers;
using Willow.Core.Helpers.Extensions;
using Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;

namespace Willow.Core;

public static class WillowStartup
{
    public static void Register(IServiceCollection services, IConfiguration configuration)
    {
        RegisterServices(services);
        Configure(services, configuration);
    }
    
    public static void Start(IServiceProvider serviceProvider)
    {
        var eventRegistrar = serviceProvider.GetRequiredService<IEventRegistrar>();
        var eventDispatcher = serviceProvider.GetRequiredService<IEventDispatcher>();
        eventRegistrar.RegisterEventsFromAssemblies([typeof(WillowStartup).Assembly]);
        RegisterInterceptors(eventDispatcher);
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

    private static void RegisterInterceptors(IEventDispatcher eventDispatcher)
    {
        var registrars = typeof(IInterceptorRegistrar).GetAllDerivingInOwnAssembly();
        foreach (var registrar in registrars)
        {
            var registrationMethod = registrar.GetMethod(nameof(IInterceptorRegistrar.RegisterInterceptor));
            registrationMethod?.Invoke(null, [eventDispatcher]);
        }
    }
}