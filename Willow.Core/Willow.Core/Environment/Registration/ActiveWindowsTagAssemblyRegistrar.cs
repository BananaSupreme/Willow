using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Willow.Environment.Abstractions;
using Willow.Environment.Attributes;
using Willow.Environment.Models;
using Willow.Helpers.Logging.Loggers;
using Willow.Registration;

namespace Willow.Environment.Registration;

/// <summary>
/// Registers all the <see cref="ActivateTagsAttribute" /> in the assembly.
/// </summary>
internal sealed class ActiveWindowsTagAssemblyRegistrar : IAssemblyRegistrar
{
    private IActiveWindowTagStorage? _activeWindowTagStorage;
    private readonly ILogger<ActiveWindowsTagAssemblyRegistrar> _log;

    public ActiveWindowsTagAssemblyRegistrar(ILogger<ActiveWindowsTagAssemblyRegistrar> log)
    {
        _log = log;
    }

    public ActiveWindowsTagAssemblyRegistrar(IActiveWindowTagStorage activeWindowTagStorage,
                                             ILogger<ActiveWindowsTagAssemblyRegistrar> log)
    {
        _activeWindowTagStorage = activeWindowTagStorage;
        _log = log;
    }

    public Task StartAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        _activeWindowTagStorage ??= serviceProvider.GetRequiredService<IActiveWindowTagStorage>();
        var tags = GetTags(assembly);
        _log.FoundTags(tags);
        _activeWindowTagStorage.Add(tags);
        return Task.CompletedTask;
    }

    public Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        _activeWindowTagStorage ??= serviceProvider.GetRequiredService<IActiveWindowTagStorage>();
        var tags = GetTags(assembly);
        _log.RemovingTags(tags);
        _activeWindowTagStorage.Remove(tags);
        return Task.CompletedTask;
    }

    private static Dictionary<string, Tag[]> GetTags(Assembly assembly)
    {
        var tags = assembly.GetCustomAttributes(typeof(ActivateTagsAttribute))
                           .Cast<ActivateTagsAttribute>()
                           .GroupBy(static tag => tag.ProcessName)
                           .ToDictionary(static grouping => grouping.Key,
                                         static grouping => grouping.SelectMany(static tags => tags.Tags)
                                                                    .Select(static tag => new Tag(tag))
                                                                    .ToArray());
        return tags;
    }
}

internal static partial class ActiveWindowsTagAssemblyRegistrarLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Found tags to be added:\r\n{tags}")]
    public static partial void FoundTags(this ILogger logger, JsonLogger<Dictionary<string, Tag[]>> tags);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Found tags to be removed:\r\n{tags}")]
    public static partial void RemovingTags(this ILogger logger, JsonLogger<Dictionary<string, Tag[]>> tags);
}
