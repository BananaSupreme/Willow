using System.Reflection;

using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Attributes;
using Willow.Core.Environment.Models;
using Willow.Core.Registration.Abstractions;
using Willow.Helpers.Logging.Loggers;

namespace Willow.Core.Environment.Registration;

/// <summary>
/// Registers all the <see cref="ActivateTagsAttribute" /> in the assembly.
/// </summary>
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
        var tags = assemblies
                   .SelectMany(static assembly => assembly.GetCustomAttributes(typeof(ActivateTagsAttribute))
                                                          .Cast<ActivateTagsAttribute>())
                   .GroupBy(static tag => tag.ProcessName)
                   .ToDictionary(static grouping => grouping.Key,
                                 static grouping => grouping.SelectMany(static tags => tags.Tags)
                                                            .Select(static tag => new Tag(tag))
                                                            .ToArray());

        _log.FoundTags(tags);
        _activeWindowTagStorage.Set(tags);
    }
}

internal static partial class ActiveWindowsTagAssemblyRegistrarLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Found tags:\r\n{tags}")]
    public static partial void FoundTags(this ILogger logger, JsonLogger<Dictionary<string, Tag[]>> tags);
}
