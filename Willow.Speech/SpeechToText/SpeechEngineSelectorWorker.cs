using Microsoft.Extensions.Hosting;

using Willow.Core.Environment.Abstractions;
using Willow.Core.Eventing.Abstractions;
using Willow.Core.Settings.Abstractions;
using Willow.Core.Settings.Events;
using Willow.Helpers.Logging.Loggers;
using Willow.Speech.SpeechToText.Abstractions;
using Willow.Speech.SpeechToText.Settings;

namespace Willow.Speech.SpeechToText;

internal sealed class SpeechEngineSelectorWorker
    : IHostedService, IEventHandler<SettingsUpdatedEvent<SelectedSpeechEngineSettings>>
{
    private readonly ISpeechToTextEngine[] _localSpeechEngines;
    private readonly ILogger<SpeechEngineSelectorWorker> _log;
    private readonly ISettings<SelectedSpeechEngineSettings> _settings;

    public SpeechEngineSelectorWorker(IEnumerable<ISpeechToTextEngine> localSpeechEngines,
                                      ISettings<SelectedSpeechEngineSettings> settings,
                                      IEnvironmentStateProvider environmentStateProvider,
                                      ILogger<SpeechEngineSelectorWorker> log)
    {
        _localSpeechEngines = localSpeechEngines.Where(x => x.SupportedOss.HasFlag(environmentStateProvider.ActiveOs))
                                                .ToArray();
        _settings = settings;
        _log = log;
    }

    public async Task HandleAsync(SettingsUpdatedEvent<SelectedSpeechEngineSettings> @event)
    {
        foreach (var engine in _localSpeechEngines.Where(static x => x.IsRunning))
        {
            _log.StoppingEngine(new TypeNameLogger<ISpeechToTextEngine>(engine));
            await engine.StopAsync();
        }

        await StartSelectedEngineAsync(CancellationToken.None);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await StartSelectedEngineAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var engine in _localSpeechEngines)
        {
            await engine.StopAsync(cancellationToken);
        }
    }

    private async Task StartSelectedEngineAsync(CancellationToken cancellationToken)
    {
        var speechEngine
            = _localSpeechEngines.First(x => x.Name == _settings.CurrentValue.SelectedSpeechEngine.ToString());
        _log.StartingEngine(new TypeNameLogger<ISpeechToTextEngine>(speechEngine));
        await speechEngine.StartAsync(cancellationToken);
    }
}

internal static partial class SpeechEngineSelectorWorkerLoggerExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "starting engine of type ({engine})")]
    public static partial void StartingEngine(this ILogger log, TypeNameLogger<ISpeechToTextEngine> engine);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "stopping engine of type ({engine})")]
    public static partial void StoppingEngine(this ILogger log, TypeNameLogger<ISpeechToTextEngine> engine);
}
