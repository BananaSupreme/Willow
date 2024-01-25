using Willow.Core.Environment.Abstractions;
using Willow.Core.Eventing.Abstractions;
using Willow.Core.Registration.Abstractions;
using Willow.Core.Settings.Abstractions;
using Willow.Core.Settings.Events;
using Willow.Helpers.Logging.Loggers;
using Willow.Speech.SpeechToText.Abstractions;
using Willow.Speech.SpeechToText.Settings;

namespace Willow.Speech.SpeechToText;

internal sealed class SpeechEngineSelectorWorker
    : IBackgroundWorker, IEventHandler<SettingsUpdatedEvent<SelectedSpeechEngineSettings>>
{
    private readonly ILogger<SpeechEngineSelectorWorker> _log;
    private readonly ISettings<SelectedSpeechEngineSettings> _settings;
    private readonly IEnvironmentStateProvider _environmentStateProvider;
    private readonly ICollectionProvider<ISpeechToTextEngine> _speechEngines;

    public SpeechEngineSelectorWorker(ICollectionProvider<ISpeechToTextEngine> speechEngines,
                                      ISettings<SelectedSpeechEngineSettings> settings,
                                      IEnvironmentStateProvider environmentStateProvider,
                                      ILogger<SpeechEngineSelectorWorker> log)
    {
        _speechEngines = speechEngines;
        _settings = settings;
        _environmentStateProvider = environmentStateProvider;
        _log = log;
    }

    public async Task HandleAsync(SettingsUpdatedEvent<SelectedSpeechEngineSettings> @event)
    {
        var localEngines = _speechEngines.GetLocal(_environmentStateProvider).Where(static x => x.IsRunning);
        foreach (var engine in localEngines)
        {
            _log.StoppingEngine(new TypeNameLogger<ISpeechToTextEngine>(engine));
            await engine.StopAsync();
        }

        await StartSelectedEngineAsync(CancellationToken.None);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_settings.CurrentValue.SelectedSpeechEngine is null)
        {
            return;
        }

        await StartSelectedEngineAsync(cancellationToken);
    }

    public async Task StopAsync()
    {
        foreach (var engine in _speechEngines.GetLocal(_environmentStateProvider))
        {
            await engine.StopAsync();
        }
    }

    private async Task StartSelectedEngineAsync(CancellationToken cancellationToken)
    {
        var speechEngine = _speechEngines.GetLocal(_environmentStateProvider)
                                         .First(x => x.Name == _settings.CurrentValue.SelectedSpeechEngine);
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

file static class LinqExtensions
{
    public static IEnumerable<ISpeechToTextEngine> GetLocal(
        this ICollectionProvider<ISpeechToTextEngine> collectionProvider,
        IEnvironmentStateProvider environmentStateProvider)
    {
        return collectionProvider.Get().Where(x => x.SupportedOss.HasFlag(environmentStateProvider.ActiveOs));
    }
}
