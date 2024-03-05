using System.Reflection;

using Willow.Registration;

namespace PluginA;

public class PluginAAssemblyRegistrar : IAssemblyRegistrar
{
    public Type[] ExtensionTypes => [typeof(ITestExtensionType)];

    public Task StartAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        return Task.CompletedTask;
    }
}
