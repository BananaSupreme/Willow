namespace Willow.Registration;

/// <summary>
/// Loads and unloads plugin in the system after they have been downloaded.
/// </summary>
public interface IPluginLoader
{
    /// <summary>
    /// The name of the folder plugins are stored in.
    /// </summary>
    internal static readonly string PluginFolderPath = Path.Combine($"{System.Environment.CurrentDirectory}", "plugins");

    /// <summary>
    /// This loads the plugin in the system and feeds it into <see cref="IAssemblyRegistrationEntry"/>. <br/>
    /// The plugin dependencies are isolated unless it depends on other plugins, in which case they are loaded as shared
    /// assemblies.
    /// </summary>
    /// <remarks>
    /// The plugin is expected to be found in <see cref="PluginFolderPath"/> in a directory named after
    /// <paramref name="pluginName"/> and the main entry point should be called {{pluginName}}.dll .
    /// </remarks>
    /// <param name="pluginName">The name of the plugin to load.</param>
    public Task LoadPluginAsync(string pluginName);

    /// <summary>
    /// Unloads the plugin from the system and unregisters it.
    /// </summary>
    /// <param name="pluginName">The name of the plugin to unload.</param>
    public Task UnloadPluginAsync(string pluginName);
}
