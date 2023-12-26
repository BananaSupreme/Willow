using Microsoft.Extensions.Hosting;

using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Eventing.Events;
using Willow.Core.Environment.Models;
using Willow.Core.Eventing.Abstractions;
using Willow.Core.Logging.Loggers;
using Willow.Core.Logging.Settings;

namespace Willow.Core.Environment;

internal class ActiveWindowDetectorWorker : BackgroundService
{
    private readonly IActiveWindowDetector _activeWindowDetector;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IOptionsMonitor<PrivacySettings> _privacySettings;
    private readonly ILogger<ActiveWindowDetectorWorker> _log;
    private ActiveWindowInfo _currentWindow;

    public ActiveWindowDetectorWorker(IActiveWindowDetector activeWindowDetector,
                                      IEventDispatcher eventDispatcher,
                                      IOptionsMonitor<PrivacySettings> privacySettings,  
                                      ILogger<ActiveWindowDetectorWorker> log)
    {
        _activeWindowDetector = activeWindowDetector;
        _eventDispatcher = eventDispatcher;
        _privacySettings = privacySettings;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(async () => await ExecuteInternalAsync(stoppingToken), stoppingToken);
    }

    private async Task ExecuteInternalAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _log.CheckingWindow();
            var window = _activeWindowDetector.GetActiveWindow();
            _log.WindowFound(new(window, _privacySettings.CurrentValue.AllowLoggingActiveWindow));
            if (window != _currentWindow)
            {
                _log.WindowChanged(new(_currentWindow, _privacySettings.CurrentValue.AllowLoggingActiveWindow), new(window, _privacySettings.CurrentValue.AllowLoggingActiveWindow));
                _currentWindow = window;
                _eventDispatcher.Dispatch(new ActiveWindowChangedEvent(window));
            }

            await Task.Delay(300, stoppingToken);
        }
    }
}

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Trace,
        Message = "Checking the current active window.")]
    public static partial void CheckingWindow(this ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Trace,
        Message = "Window found ({activeWindowInfo})")]
    public static partial void WindowFound(this ILogger logger, RedactingLogger<ActiveWindowInfo> activeWindowInfo);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Window ({newWindow}) detected to be different from existing ({oldWindow})")]
    public static partial void WindowChanged(this ILogger logger, RedactingLogger<ActiveWindowInfo> oldWindow, RedactingLogger<ActiveWindowInfo> newWindow);
}