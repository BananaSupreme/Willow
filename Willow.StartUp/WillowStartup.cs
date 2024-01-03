﻿using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

using Willow.BuiltInCommands;
using Willow.Core;
using Willow.Core.Registration.Abstractions;
using Willow.DeviceAutomation;
using Willow.Helpers.Extensions;
using Willow.Speech;
using Willow.WhisperServer;

namespace Willow.StartUp;

public static class WillowStartup
{
    private static readonly Assembly[] _registeredAssemblies =
    [
        typeof(ICoreAssemblyMarker).Assembly,
        typeof(ISpeechAssemblyMarker).Assembly,
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
        var registrar = provider.GetRequiredService<IAssemblyRegistrationEntry>();
        registrar.RegisterAssemblies(_registeredAssemblies);
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