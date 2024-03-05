namespace Willow.Registration.Exceptions;

public sealed class PluginNotFoundException : InvalidOperationException
{
    public string PluginName { get; }

    public PluginNotFoundException(string pluginName) : base($"The plugin was never loaded ({pluginName})")
    {
        PluginName = pluginName;
    }
}
