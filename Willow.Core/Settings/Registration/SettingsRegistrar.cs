using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using Willow.Core.Registration.Abstractions;
using Willow.Helpers.Extensions;

namespace Willow.Core.Settings.Registration;

internal sealed class SettingsRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(typeof(ISettings<>), typeof(Settings<>));
        services.AddSingleton<IQueuedFileWriter, FileWritingWorker>();
        services.AddSingleton<IHostedService>(provider => (IHostedService)provider.GetRequiredService<IQueuedFileWriter>());
    }
}