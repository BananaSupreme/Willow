using System.Reflection;

using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Attributes;
using Willow.Core.Environment.Models;
using Willow.Core.Logging.Loggers;
using Willow.Core.Registration.Abstractions;

namespace Willow.Core.Environment.Registration;

internal sealed class ActiveWindowsTagAssemblyRegistrar : IAssemblyRegistrar
{
    private readonly IActiveWindowTagStorage _activeWindowTagStorage;
    private readonly ILogger<ActiveWindowsTagAssemblyRegistrar> _log;

    public ActiveWindowsTagAssemblyRegistrar(IActiveWindowTagStorage activeWindowTagStorage,
                                             ILogger<ActiveWindowsTagAssemblyRegistrar> log)
    {
        _activeWindowTagStorage = activeWindowTagStorage;
        _log = log;
    }

    public void RegisterFromAssemblies(Assembly[] assemblies)
    {
        var tags = assemblies.SelectMany(assembly =>
                                 assembly.GetCustomAttributes(typeof(ActivateTagsAttribute))
                                         .Cast<ActivateTagsAttribute>())
                             .GroupBy(tag => tag.ProcessName)
                             .ToDictionary(
                                 grouping => grouping.Key,
                                 grouping => grouping.SelectMany(tags => tags.Tags)
                                                     .Select(tag => new Tag(tag))
                                                     .ToArray());

        _log.FoundTags(tags);
        _activeWindowTagStorage.Set(tags);
    }
}

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Found tags:\r\n{tags}")]
    public static partial void FoundTags(this ILogger logger, JsonLogger<Dictionary<string, Tag[]>> tags);
}