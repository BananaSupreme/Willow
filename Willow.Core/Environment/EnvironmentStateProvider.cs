using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Enums;
using Willow.Core.Environment.Models;
using Willow.Core.Privacy.Settings;
using Willow.Helpers.Logging.Loggers;

namespace Willow.Core.Environment;

internal sealed class EnvironmentStateProvider : IEnvironmentStateProvider
{
    private readonly ILogger<EnvironmentStateProvider> _log;
    private readonly ISettings<PrivacySettings> _privacySettings;
    private Tag[]? _cache;
    private ActivationMode _activationMode = ActivationMode.Command;
    private readonly string _operatingSystem = System.Environment.OSVersion.Platform.ToString();
    private Tag[] _environmentTags = [];
    private ActiveWindowInfo _activeWindow = new(string.Empty);

    public IReadOnlyList<Tag> Tags => _cache ?? RestoreCache();

    public EnvironmentStateProvider(ILogger<EnvironmentStateProvider> log,
                                    ISettings<PrivacySettings> privacySettings)
    {
        _log = log;
        _privacySettings = privacySettings;
    }

    private readonly HashSet<Tag> _customTags = [];

    public void SetActiveWindowInfo(ActiveWindowInfo activeWindow)
    {
        _log.ActiveWindowChanged(new(activeWindow, _privacySettings.CurrentValue.AllowLoggingActiveWindow));
        _activeWindow = activeWindow;
        RestoreCache();
    }

    public void SetActivationMode(ActivationMode activationMode)
    {
        _log.ActivationModeChanged(activationMode);
        _activationMode = activationMode;
        RestoreCache();
    }

    public void SetEnvironmentTags(Tag[] tags)
    {
        _log.EnvironmentTagsChanged(new(tags, _privacySettings.CurrentValue.AllowLoggingActiveWindow));
        _environmentTags = tags;
        RestoreCache();
    }

    public void ActivateTag(Tag tag)
    {
        _log.TagActivated(new(tag, _privacySettings.CurrentValue.AllowLoggingCommands));
        _customTags.Add(tag);
        RestoreCache();
    }

    public void DeactivateTag(Tag tag)
    {
        _log.TagDeactivated(new(tag, _privacySettings.CurrentValue.AllowLoggingCommands));
        _customTags.Remove(tag);
        RestoreCache();
    }

    private Tag[] RestoreCache()
    {
        _cache =
        [
            new(_operatingSystem),
            new(_activationMode.ToString()),
            .. _environmentTags,
            .. _customTags,
            new(_activeWindow.ProcessName)
        ];
        _log.CacheRestored(new(_cache,
            _privacySettings.CurrentValue.AllowLoggingCommands
            && _privacySettings.CurrentValue.AllowLoggingActiveWindow));
        return _cache;
    }
}

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Active window changed ({activeWindow})")]
    public static partial void ActiveWindowChanged(this ILogger log, RedactingLogger<ActiveWindowInfo> activeWindow);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Activation mode changed ({activationMode})")]
    public static partial void ActivationModeChanged(this ILogger log, ActivationMode activationMode);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Environment tags changed: {tags}")]
    public static partial void EnvironmentTagsChanged(this ILogger log, RedactingLogger<EnumeratorLogger<Tag>> tags);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Tag activated ({tag})")]
    public static partial void TagActivated(this ILogger log, RedactingLogger<Tag> tag);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Tag deactivated ({tag})")]
    public static partial void TagDeactivated(this ILogger log, RedactingLogger<Tag> tag);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Trace,
        Message = "Cache restored as a consequence of tags changing: {cache}")]
    public static partial void CacheRestored(this ILogger log, RedactingLogger<EnumeratorLogger<Tag>> cache);
}