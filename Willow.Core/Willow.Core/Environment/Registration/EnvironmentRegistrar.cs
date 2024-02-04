using Microsoft.Extensions.DependencyInjection;

using Willow.Environment.Abstractions;
using Willow.Environment.ActiveWindowDetectors;
using Willow.Helpers.OS;
using Willow.Registration;

namespace Willow.Environment.Registration;

internal sealed class EnvironmentRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        RegisterActiveWindow(services);
        services.AddSingleton<IEnvironmentStateProvider, EnvironmentStateProvider>();
        services.AddSingleton<IActiveWindowTagStorage, ActiveWindowTagStorage>();
    }

    private static void RegisterActiveWindow(IServiceCollection services)
    {
        OsHelpers.MatchOs(services.AddSingleton<IActiveWindowDetector, WindowsActiveWindowDetector>);
    }
}
