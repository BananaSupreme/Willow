using Microsoft.Extensions.Hosting;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.Settings.Abstractions;
using Willow.Core.Settings.Events;
using Willow.Speech.SpeechToText.Abstractions;
using Willow.Speech.SpeechToText.Settings;

namespace Willow.Speech.SpeechToText;

internal sealed class SpeechEngineSelectorWorker : IHostedService,
    IEventHandler<SettingsUpdatedEvent<SelectedSpeechEngineSettings>>
{
    private readonly IEnumerable<ISpeechToTextEngine> _localSpeechEngines;
    private readonly ISettings<SelectedSpeechEngineSettings> _settings;

    public SpeechEngineSelectorWorker(IEnumerable<ISpeechToTextEngine> localSpeechEngines,
                                      ISettings<SelectedSpeechEngineSettings> settings)
    {
        _localSpeechEngines = localSpeechEngines;
        _settings = settings;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await StartSelectedEngineAsync(cancellationToken);
    }

    public async Task HandleAsync(SettingsUpdatedEvent<SelectedSpeechEngineSettings> @event)
    {
        foreach (var engine in _localSpeechEngines.Where(x => x.IsRunning))
        {
            await engine.StopAsync();
        }
        await StartSelectedEngineAsync(CancellationToken.None);
    }
    
    private async Task StartSelectedEngineAsync(CancellationToken cancellationToken)
    {
        var speechEngine = _localSpeechEngines
            .First(x => x.Name == _settings.CurrentValue.SelectedSpeechEngine.ToString());
        await speechEngine.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var engine in _localSpeechEngines)
        {
            await engine.StopAsync(cancellationToken);
        }
    }
}