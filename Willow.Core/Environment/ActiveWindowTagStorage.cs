using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Eventing.Handlers;
using Willow.Core.Environment.Models;
using Willow.Core.Privacy.Settings;
using Willow.Helpers.Logging.Loggers;

namespace Willow.Core.Environment;

internal sealed class ActiveWindowTagStorage : IActiveWindowTagStorage
{
    private readonly ILogger<ActiveWindowChangedEventHandler> _log;
    private readonly ISettings<PrivacySettings> _privacySettings;
    private readonly Dictionary<string, Tag[]> _storage = [];

    public ActiveWindowTagStorage(ILogger<ActiveWindowChangedEventHandler> log,
                                  ISettings<PrivacySettings> privacySettings)
    {
        _log = log;
        _privacySettings = privacySettings;
    }

    public Tag[] GetByProcessName(string processName)
    {
        _log.LookingForProcessName(new RedactingLogger<string>(processName,
                                                               _privacySettings.CurrentValue.AllowLoggingActiveWindow));
        if (_storage.TryGetValue(processName, out var tags))
        {
            _log.ProcessFoundRegistered(
                new RedactingLogger<string>(processName, _privacySettings.CurrentValue.AllowLoggingActiveWindow));
            return tags;
        }

        _log.ProcessNotFoundRegistered(
            new RedactingLogger<string>(processName, _privacySettings.CurrentValue.AllowLoggingActiveWindow));
        return [];
    }

    public void Add(IDictionary<string, Tag[]> tags)
    {
        foreach (var (key, value) in tags)
        {
            if (_storage.TryGetValue(key, out var existingTags))
            {
                _storage[key] = existingTags.Union(value).Distinct().ToArray();
                continue;
            }

            _storage.Add(key, value);
        }
    }

    public void Remove(IDictionary<string, Tag[]> tags)
    {
        foreach (var (key, value) in tags)
        {
            if (!_storage.TryGetValue(key, out var existingTags))
            {
                continue;
            }

            var remainingTags = existingTags.Except(value).ToArray();
            if (remainingTags.Length == 0)
            {
                _storage.Remove(key);
                continue;
            }

            _storage[key] = remainingTags;
        }
    }
}

internal static partial class ActiveWindowTagStorageLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Trace, Message = "Looking for tags on process ({processName})")]
    public static partial void LookingForProcessName(this ILogger logger, RedactingLogger<string> processName);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Tags found on process ({processName})")]
    public static partial void ProcessFoundRegistered(this ILogger logger, RedactingLogger<string> processName);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Tags not found on process ({processName})")]
    public static partial void ProcessNotFoundRegistered(this ILogger logger, RedactingLogger<string> processName);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Found tags:\r\n{tags}")]
    public static partial void FoundTags(this ILogger logger, JsonLogger<Dictionary<string, Tag[]>> tags);
}
