using Microsoft.Extensions.Hosting;

using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Eventing.Events;
using Willow.Core.Environment.Models;
using Willow.Core.Eventing.Abstractions;

namespace Willow.Core.Environment;

internal class ActiveWindowDetectorWorker : BackgroundService
{
    private readonly IActiveWindowDetector _activeWindowDetector;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly ILogger<ActiveWindowDetectorWorker> _log;
    private ActiveWindowInfo _currentWindow;

    public ActiveWindowDetectorWorker(IActiveWindowDetector activeWindowDetector,
                                      IEventDispatcher eventDispatcher,
                                      ILogger<ActiveWindowDetectorWorker> log)
    {
        _activeWindowDetector = activeWindowDetector;
        _eventDispatcher = eventDispatcher;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _log.CheckingWindow();
            var window = _activeWindowDetector.GetActiveWindow();
            _log.WindowFound(window);
            if (window != _currentWindow)
            {
                _log.WindowChanged(_currentWindow, window);
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
    public static partial void WindowFound(this ILogger logger, ActiveWindowInfo activeWindowInfo);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Window ({newWindow}) detected to be different from existing ({oldWindow})")]
    public static partial void WindowChanged(this ILogger logger, ActiveWindowInfo oldWindow, ActiveWindowInfo newWindow);
}