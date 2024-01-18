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
    private IDictionary<string, Tag[]> _storage = new Dictionary<string, Tag[]>();

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

    public void Set(IDictionary<string, Tag[]> tags)
    {
        _storage = tags;
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
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Found tags:\r\n{tags}")]
    public static partial void FoundTags(this ILogger logger, JsonLogger<Dictionary<string, Tag[]>> tags);
}
