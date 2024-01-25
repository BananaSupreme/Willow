using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.ActiveWindowDetectors;
using Willow.Core.Registration.Abstractions;
using Willow.Helpers.OS;

namespace Willow.Core.Environment.Registration;

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
