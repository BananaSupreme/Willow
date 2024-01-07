using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

using Willow.BuiltInCommands;
using Willow.Core;
using Willow.Core.Registration.Abstractions;
using Willow.DeviceAutomation;
using Willow.Helpers.Extensions;
using Willow.Speech;
using Willow.Vosk;
using Willow.WhisperServer;

namespace Willow.StartUp;

public static class WillowStartup
{
    private static readonly Assembly[] _registeredAssemblies =
    [
        typeof(ICoreAssemblyMarker).Assembly,
        typeof(ISpeechAssemblyMarker).Assembly,
        typeof(IVoskAssemblyMarker).Assembly,
        typeof(IWhisperServerAssemblyMarker).Assembly,
        typeof(IDeviceAutomationAssemblyMarker).Assembly,
        typeof(IBuiltInCommandsAssemblyMarker).Assembly,
    ];
    
    public static void Register(IServiceCollection services)
    {
        Register(_registeredAssemblies, services);
    }

    public static void Run(IServiceProvider provider)
    {
        Run(_registeredAssemblies, provider);
    }
    
    internal static void Run(Assembly[] assemblies, IServiceProvider provider)
    {
        var registrar = provider.GetRequiredService<IAssemblyRegistrationEntry>();
        registrar.RegisterAssemblies(assemblies);
    }

    internal static void Register(Assembly[] assemblies, IServiceCollection services)
    {
        var registrars =
            assemblies.SelectMany(assembly => typeof(IServiceRegistrar).GetAllDerivingInAssembly(assembly));
        foreach (var registrar in registrars)
        {
            var registrationMethod = registrar.GetMethod(nameof(IServiceRegistrar.RegisterServices));
            registrationMethod?.Invoke(null, [services]);
        }
    }
}