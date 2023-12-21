using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.ActiveWindowDetectors;
using Willow.Core.Helpers;

namespace Willow.Core.Environment;

internal class EnvironmentRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IActiveWindowDetector, EmptyActiveWindowDetector>();
        services.AddSingleton<IEnvironmentStateProvider, EnvironmentStateProvider>();
        services.AddHostedService<ActiveWindowDetectorWorker>();
    }
}