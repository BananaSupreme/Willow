using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;

namespace Willow.Core.Settings.Registration;

internal sealed class SettingsRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(typeof(ISettings<>), typeof(Settings<>));
        services.AddSingleton<IQueuedFileWriter, FileWritingWorker>();
    }
}
