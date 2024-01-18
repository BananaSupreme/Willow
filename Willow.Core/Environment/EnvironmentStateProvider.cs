using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Enums;
using Willow.Core.Environment.Models;
using Willow.Core.Privacy.Settings;
using Willow.Helpers.Logging.Loggers;

namespace Willow.Core.Environment;

internal sealed class EnvironmentStateProvider : IEnvironmentStateProvider
{
    private readonly HashSet<Tag> _customTags = [];
    private readonly ILogger<EnvironmentStateProvider> _log;
    private readonly ISettings<PrivacySettings> _privacySettings;
    private ActivationMode _activationMode = ActivationMode.Command;
    private ActiveWindowInfo _activeWindow = new(string.Empty);
    private Tag[]? _cache;
    private Tag[] _windowTags = [];

    public EnvironmentStateProvider(ILogger<EnvironmentStateProvider> log, ISettings<PrivacySettings> privacySettings)
    {
        _log = log;
        _privacySettings = privacySettings;
    }

    public SupportedOss ActiveOs { get; } = GetSupportedOss();

    public IReadOnlyList<Tag> Tags => _cache ?? RestoreCache();

    public void SetActiveWindowInfo(ActiveWindowInfo activeWindow)
    {
        _log.ActiveWindowChanged(new RedactingLogger<ActiveWindowInfo>(activeWindow,
                                                                       _privacySettings.CurrentValue
                                                                           .AllowLoggingActiveWindow));
        _activeWindow = activeWindow;
        RestoreCache();
    }

    public void SetActivationMode(ActivationMode activationMode)
    {
        _log.ActivationModeChanged(activationMode);
        _activationMode = activationMode;
        RestoreCache();
    }

    public void SetWindowTags(Tag[] tags)
    {
        _log.WindowTagsChanged(new RedactingLogger<EnumeratorLogger<Tag>>(tags,
                                                                          _privacySettings.CurrentValue
                                                                              .AllowLoggingActiveWindow));
        _windowTags = tags;
        RestoreCache();
    }

    public void ActivateTag(Tag tag)
    {
        _log.TagActivated(new RedactingLogger<Tag>(tag, _privacySettings.CurrentValue.AllowLoggingCommands));
        _customTags.Add(tag);
        RestoreCache();
    }

    public void DeactivateTag(Tag tag)
    {
        _log.TagDeactivated(new RedactingLogger<Tag>(tag, _privacySettings.CurrentValue.AllowLoggingCommands));
        _customTags.Remove(tag);
        RestoreCache();
    }

    private Tag[] RestoreCache()
    {
        _cache =
        [
            new Tag(ActiveOs.ToString()),
            new Tag(_activationMode.ToString()),
            .. _windowTags,
            .. _customTags,
            new Tag(_activeWindow.ProcessName)
        ];
        _log.CacheRestored(new RedactingLogger<EnumeratorLogger<Tag>>(_cache,
                                                                      _privacySettings.CurrentValue.AllowLoggingCommands
                                                                      && _privacySettings.CurrentValue
                                                                          .AllowLoggingActiveWindow));
        return _cache;
    }

    private static SupportedOss GetSupportedOss()
    {
        return System.Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => SupportedOss.Windows,
            _ => throw new PlatformNotSupportedException(
                     $"We do not support yet the ({System.Environment.OSVersion.Platform}) platform")
        };
    }
}

internal static partial class EnvironmentStateProviderLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Active window changed ({activeWindow})")]
    public static partial void ActiveWindowChanged(this ILogger log, RedactingLogger<ActiveWindowInfo> activeWindow);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Activation mode changed ({activationMode})")]
    public static partial void ActivationModeChanged(this ILogger log, ActivationMode activationMode);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Environment tags changed: {tags}")]
    public static partial void WindowTagsChanged(this ILogger log, RedactingLogger<EnumeratorLogger<Tag>> tags);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Tag activated ({tag})")]
    public static partial void TagActivated(this ILogger log, RedactingLogger<Tag> tag);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Tag deactivated ({tag})")]
    public static partial void TagDeactivated(this ILogger log, RedactingLogger<Tag> tag);

    [LoggerMessage(EventId = 6,
                   Level = LogLevel.Trace,
                   Message = "Cache restored as a consequence of tags changing: {cache}")]
    public static partial void CacheRestored(this ILogger log, RedactingLogger<EnumeratorLogger<Tag>> cache);
}
