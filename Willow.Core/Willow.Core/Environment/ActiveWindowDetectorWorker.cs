using Willow.Environment.Abstractions;
using Willow.Environment.Events;
using Willow.Environment.Models;
using Willow.Eventing;
using Willow.Helpers.Logging.Loggers;
using Willow.Privacy.Settings;
using Willow.Registration;
using Willow.Settings;

namespace Willow.Environment;

internal sealed class ActiveWindowDetectorWorker : IBackgroundWorker
{
    private readonly IActiveWindowDetector _activeWindowDetector;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly ILogger<ActiveWindowDetectorWorker> _log;
    private readonly ISettings<PrivacySettings> _privacySettings;
    private ActiveWindowInfo _currentWindow;

    public ActiveWindowDetectorWorker(IActiveWindowDetector activeWindowDetector,
                                      IEventDispatcher eventDispatcher,
                                      ISettings<PrivacySettings> privacySettings,
                                      ILogger<ActiveWindowDetectorWorker> log)
    {
        _activeWindowDetector = activeWindowDetector;
        _eventDispatcher = eventDispatcher;
        _privacySettings = privacySettings;
        _log = log;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(async () => await ExecuteInternalAsync(cancellationToken), cancellationToken);
    }

    public Task StopAsync()
    {
        return Task.CompletedTask;
    }

    private async Task ExecuteInternalAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _log.CheckingWindow();
            var window = _activeWindowDetector.GetActiveWindow();
            _log.WindowFound(
                new RedactingLogger<ActiveWindowInfo>(window, _privacySettings.CurrentValue.AllowLoggingActiveWindow));
            if (window != _currentWindow)
            {
                _log.WindowChanged(
                    new RedactingLogger<ActiveWindowInfo>(_currentWindow,
                                                          _privacySettings.CurrentValue.AllowLoggingActiveWindow),
                    new RedactingLogger<ActiveWindowInfo>(window,
                                                          _privacySettings.CurrentValue.AllowLoggingActiveWindow));
                _currentWindow = window;
                _eventDispatcher.Dispatch(new ActiveWindowChangedEvent(window));
            }

            await Task.Delay(300, CancellationToken.None);
        }
    }
}

internal static partial class ActiveWindowDetectorWorkerLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Trace, Message = "Checking the current active window.")]
    public static partial void CheckingWindow(this ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Trace, Message = "Window found ({activeWindowInfo})")]
    public static partial void WindowFound(this ILogger logger, RedactingLogger<ActiveWindowInfo> activeWindowInfo);

    [LoggerMessage(EventId = 3,
                   Level = LogLevel.Information,
                   Message = "Window ({newWindow}) detected to be different from existing ({oldWindow})")]
    public static partial void WindowChanged(this ILogger logger,
                                             RedactingLogger<ActiveWindowInfo> oldWindow,
                                             RedactingLogger<ActiveWindowInfo> newWindow);
}
