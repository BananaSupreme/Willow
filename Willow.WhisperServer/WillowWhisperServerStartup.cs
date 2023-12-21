using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Helpers;
using Willow.Core.SpeechCommands.SpeechRecognition.SpeechToText.Abstractions;

namespace Willow.WhisperServer;

public static class WillowWhisperServerStartup
{
    private static WhisperEngine? _speechToTextEngine;
    public static void Register(IServiceCollection services, IConfiguration configuration)
    {
        RegisterServices(services);
        Configure(services, configuration);
    }
    
    public static void Start(IServiceProvider serviceProvider)
    {
        _speechToTextEngine = serviceProvider.GetRequiredService<ISpeechToTextEngine>() as WhisperEngine;
    }

    public static void Stop()
    {
        _speechToTextEngine?.Dispose();
    }
    
    private static void RegisterServices(IServiceCollection services)
    {
        var registrars = typeof(IServiceRegistrar).GetAllDeriving();
        foreach (var registrar in registrars)
        {
            var registrationMethod = registrar.GetMethod(nameof(IServiceRegistrar.RegisterServices));
            registrationMethod?.Invoke(null, [services]);
        }
    }
    
    private static void Configure(IServiceCollection services, IConfiguration configuration)
    {
        var registrars = typeof(IConfigurationRegistrar).GetAllDeriving();
        foreach (var registrar in registrars)
        {
            var registrationMethod = registrar.GetMethod(nameof(IConfigurationRegistrar.RegisterConfiguration));
            registrationMethod?.Invoke(null, [services, configuration]);
        }
    }
    
    private static Type[] GetAllDeriving(this Type type)
    {
        return typeof(WillowWhisperServerStartup).Assembly.GetTypes()
                       .Where(t => t.IsConcrete())
                       .Where(t => t.IsAssignableTo(type))
                       .ToArray();
    }
    
    private static bool IsConcrete(this Type type)
    {
        return !type.IsInterface && !type.IsAbstract;
    }
}