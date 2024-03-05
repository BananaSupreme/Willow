using System.Reflection;
using System.Runtime.Loader;

using Willow.Helpers.Logging.Extensions;
using Willow.Helpers.Logging.Loggers;
using Willow.Registration.Exceptions;

namespace Willow.Registration;

internal sealed class PluginLoader : IPluginLoader
{
    private readonly IAssemblyRegistrationEntry _assemblyRegistrationEntry;
    private readonly ILogger<PluginLoader> _log;
    private static readonly Assembly _coreAbstractionsLibrary = typeof(IPluginLoader).Assembly;
    private readonly Dictionary<string, PluginContext> _cache = [];

    public PluginLoader(IAssemblyRegistrationEntry assemblyRegistrationEntry, ILogger<PluginLoader> log)
    {
        _assemblyRegistrationEntry = assemblyRegistrationEntry;
        _log = log;
    }

    public async Task LoadPluginAsync(string pluginName)
    {
        using var _ = _log.AddContext(nameof(pluginName), pluginName);
        var assemblyPath = Path.Combine(IPluginLoader.PluginFolderPath, pluginName, $"{pluginName}.dll");
        _log.PluginLoading();
        var sharedAssemblies = GetSharedAssemblies();
        var loadContext = new PluginLoadContext(sharedAssemblies, pluginName, assemblyPath, _log);
        _log.SharedAssemblies(new EnumeratorLogger<Assembly>(sharedAssemblies));
        var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
        try
        {
            var assemblyId = await _assemblyRegistrationEntry.RegisterAssemblyAsync(assembly);
            _log.PluginLoaded();
            _cache.Add(pluginName, new PluginContext(assemblyId, assembly, loadContext));
        }
        catch (Exception ex)
        {
            _log.PluginLoadingFailed(ex);
            try
            {
                loadContext.Unload();
            }
            catch (Exception ex2)
            {
                //Nothing to do here
                _log.UnloadingLoadedPluginFailed(ex2);
            }

            throw;
        }
    }

    private Assembly[] GetSharedAssemblies()
    {
        return _cache.Select(static x => x.Value.MainAssembly).Append(_coreAbstractionsLibrary).ToArray();
    }

    public async Task UnloadPluginAsync(string pluginName)
    {
        using var _ = _log.AddContext(nameof(pluginName), pluginName);
        _log.UnloadingPlugin();
        if (!_cache.TryGetValue(pluginName, out var pluginContext))
        {
            _log.PluginNotFound();
            throw new PluginNotFoundException(pluginName);
        }

        try
        {
            await _assemblyRegistrationEntry.UnregisterAssemblyAsync(pluginContext.AssemblyId);
            _log.UnloadedPlugin();
        }
        catch (Exception ex)
        {
            _log.PluginUnloadingFailed(ex);
            throw;
        }
        finally
        {
            _cache.Remove(pluginName);
            pluginContext.LoadContext.Unload();
        }
    }

    private sealed class PluginLoadContext : AssemblyLoadContext
    {
        private readonly Assembly[] _sharedAssemblies;
        private readonly string _pluginName;
        private readonly ILogger _log;
        private readonly AssemblyDependencyResolver _resolver;

        public PluginLoadContext(Assembly[] sharedAssemblies, string pluginName, string pluginPath, ILogger log) : base(
            isCollectible: true)
        {
            _sharedAssemblies = sharedAssemblies;
            _pluginName = pluginName;
            _log = log;
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            using var _ = _log.AddContext("pluginName", _pluginName);
            using var __ = _log.AddContext("assemblyName", assemblyName.FullName);
            _log.LoadingAssembly();
            var sharedAssembly = _sharedAssemblies.FirstOrDefault(x => x.FullName == assemblyName.FullName);
            if (sharedAssembly is not null)
            {
                _log.LoadedAssemblyAsSharedAssembly();
                return sharedAssembly;
            }

            return LoadFromContext(assemblyName);
        }

        private Assembly? LoadFromContext(AssemblyName name)
        {
            var assemblyPath = _resolver.ResolveAssemblyToPath(name);
            _log.AssemblyPathResolution(assemblyPath is not null);
            return assemblyPath is not null ? LoadFromAssemblyPath(assemblyPath) : null;
        }
    }

    private readonly record struct PluginContext(Guid AssemblyId,
                                                 Assembly MainAssembly,
                                                 PluginLoadContext LoadContext);
}

internal static partial class PluginLoaderLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Started loading new plugin.")]
    public static partial void PluginLoading(this ILogger logger);

    [LoggerMessage(EventId = 2,
                   Level = LogLevel.Debug,
                   Message = "Loading the plugin using the following assemblies: {assemblies}")]
    public static partial void SharedAssemblies(this ILogger logger, EnumeratorLogger<Assembly> assemblies);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Loaded the plugin successfully.")]
    public static partial void PluginLoaded(this ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Plugin loading failed")]
    public static partial void PluginLoadingFailed(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 5,
                   Level = LogLevel.Error,
                   Message = "Plugin loading failed, afterwords plugin unloading commenced and also failed.")]
    public static partial void UnloadingLoadedPluginFailed(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Started unloading new plugin.")]
    public static partial void UnloadingPlugin(this ILogger logger);

    [LoggerMessage(EventId = 7, Level = LogLevel.Error, Message = "Unable to locate the plugin.")]
    public static partial void PluginNotFound(this ILogger logger);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "Unloaded the plugin successfully.")]
    public static partial void UnloadedPlugin(this ILogger logger);

    [LoggerMessage(EventId = 9, Level = LogLevel.Error, Message = "Plugin unloading failed")]
    public static partial void PluginUnloadingFailed(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 10, Level = LogLevel.Trace, Message = "Started loading a dependency.")]
    public static partial void LoadingAssembly(this ILogger logger);
    
    [LoggerMessage(EventId = 11, Level = LogLevel.Trace, Message = "Assembly recognized as a shared assembly.")]
    public static partial void LoadedAssemblyAsSharedAssembly(this ILogger logger);
    
    [LoggerMessage(EventId = 12, Level = LogLevel.Trace, Message = "Attempted to resolve assembly, operation ({isSuccessful}).")]
    public static partial void AssemblyPathResolution(this ILogger logger, bool isSuccessful);
}
