using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.ActiveWindowDetectors;
using Willow.Core.Helpers;
using Willow.Core.Registration.Abstractions;

namespace Willow.Core.Environment.Registration;

internal sealed class EnvironmentRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        RegisterActiveWindow(services);
        services.AddSingleton<IAssemblyRegistrar, ActiveWindowsTagAssemblyRegistrar>();
        services.AddSingleton<IEnvironmentStateProvider, EnvironmentStateProvider>();
        services.AddSingleton<IActiveWindowTagStorage, ActiveWindowTagStorage>();
        services.AddHostedService<ActiveWindowDetectorWorker>();
    }

    private static void RegisterActiveWindow(IServiceCollection services)
    {
        OsHelpers.MatchOs(
            services.AddSingleton<IActiveWindowDetector, WindowsActiveWindowDetector>,
            services.AddSingleton<IActiveWindowDetector, EmptyActiveWindowDetector>,
            services.AddSingleton<IActiveWindowDetector, EmptyActiveWindowDetector>);
    }
}