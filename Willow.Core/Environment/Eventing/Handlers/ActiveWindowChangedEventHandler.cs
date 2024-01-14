using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Eventing.Events;
using Willow.Core.Environment.Models;
using Willow.Core.Eventing.Abstractions;
using Willow.Core.Privacy.Settings;
using Willow.Helpers.Logging.Loggers;

namespace Willow.Core.Environment.Eventing.Handlers;

internal sealed class ActiveWindowChangedEventHandler : IEventHandler<ActiveWindowChangedEvent>
{
    private readonly IActiveWindowTagStorage _activeWindowTagStorage;
    private readonly IEnvironmentStateProvider _environmentStateProvider;
    private readonly ILogger<ActiveWindowChangedEventHandler> _log;
    private readonly ISettings<PrivacySettings> _privacySettings;

    public ActiveWindowChangedEventHandler(IActiveWindowTagStorage activeWindowTagStorage,
                                           IEnvironmentStateProvider environmentStateProvider,
                                           ILogger<ActiveWindowChangedEventHandler> log,
                                           ISettings<PrivacySettings> privacySettings)
    {
        _activeWindowTagStorage = activeWindowTagStorage;
        _environmentStateProvider = environmentStateProvider;
        _log = log;
        _privacySettings = privacySettings;
    }

    public Task HandleAsync(ActiveWindowChangedEvent @event)
    {
        var tags = _activeWindowTagStorage.GetByProcessName(@event.NewWindow.ProcessName);
        
        _environmentStateProvider.SetWindowTags(tags);
        _environmentStateProvider.SetActiveWindowInfo(@event.NewWindow);
        
        _log.TagsFoundInStorage(
            new(@event.NewWindow.ProcessName, _privacySettings.CurrentValue.AllowLoggingActiveWindow),
            new(tags, _privacySettings.CurrentValue.AllowLoggingActiveWindow));
        return Task.CompletedTask;
    }
}

internal static partial class ActiveWindowChangedEventHandlerLoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Tags found from storage for process ({processName}): {tags}")]
    public static partial void TagsFoundInStorage(this ILogger logger, RedactingLogger<string> processName,
                                                  RedactingLogger<EnumeratorLogger<Tag>> tags);
}