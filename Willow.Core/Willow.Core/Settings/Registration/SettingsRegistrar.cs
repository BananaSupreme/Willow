using Microsoft.Extensions.DependencyInjection;

using Willow.Registration;
using Willow.Settings.Abstractions;

namespace Willow.Settings.Registration;

internal sealed class SettingsRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(typeof(ISettings<>), typeof(Settings<>));
        services.AddSingleton<IQueuedFileWriter, FileWritingWorker>();
    }
}
