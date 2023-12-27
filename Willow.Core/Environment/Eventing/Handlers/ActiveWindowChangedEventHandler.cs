using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Eventing.Events;
using Willow.Core.Environment.Models;
using Willow.Core.Eventing.Abstractions;
using Willow.Core.Logging.Loggers;
using Willow.Core.Logging.Settings;

namespace Willow.Core.Environment.Eventing.Handlers;

internal class ActiveWindowChangedEventHandler : IEventHandler<ActiveWindowChangedEvent>
{
    private readonly IActiveWindowTagStorage _activeWindowTagStorage;
    private readonly IEnvironmentStateProvider _environmentStateProvider;
    private readonly ILogger<ActiveWindowChangedEventHandler> _log;
    private readonly IOptionsMonitor<PrivacySettings> _privacySettings;

    public ActiveWindowChangedEventHandler(IActiveWindowTagStorage activeWindowTagStorage,
                                           IEnvironmentStateProvider environmentStateProvider,
                                           ILogger<ActiveWindowChangedEventHandler> log,
                                           IOptionsMonitor<PrivacySettings> privacySettings)
    {
        _activeWindowTagStorage = activeWindowTagStorage;
        _environmentStateProvider = environmentStateProvider;
        _log = log;
        _privacySettings = privacySettings;
    }

    public Task HandleAsync(ActiveWindowChangedEvent @event)
    {
        var tags = _activeWindowTagStorage.GetByProcessName(@event.NewWindow.ProcessName);
        
        _environmentStateProvider.EnvironmentTags = tags;
        _environmentStateProvider.ActiveWindow = @event.NewWindow;
        
        _log.TagsFoundInStorage(
            new(@event.NewWindow.ProcessName, _privacySettings.CurrentValue.AllowLoggingActiveWindow),
            new(tags, _privacySettings.CurrentValue.AllowLoggingActiveWindow));
        return Task.CompletedTask;
    }
}

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "tags found from storage for process ({processName}): {tags}")]
    public static partial void TagsFoundInStorage(this ILogger logger, RedactingLogger<string> processName,
                                                  RedactingLogger<EnumeratorLogger<Tag>> tags);
}