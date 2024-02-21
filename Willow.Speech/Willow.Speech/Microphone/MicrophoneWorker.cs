using Willow.Eventing;
using Willow.Registration;
using Willow.Settings;
using Willow.Settings.Events;
using Willow.Speech.Microphone.Abstractions;
using Willow.Speech.Microphone.Events;
using Willow.Speech.Microphone.Settings;

namespace Willow.Speech.Microphone;

internal sealed class MicrophoneWorker : IBackgroundWorker, IEventHandler<SettingsUpdatedEvent<MicrophoneSettings>>
{
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IMicrophoneAccess _microphoneAccess;
    private bool _shouldRun;

    public MicrophoneWorker(IEventDispatcher eventDispatcher,
                            IMicrophoneAccess microphoneAccess,
                            ISettings<MicrophoneSettings> settings)
    {
        _eventDispatcher = eventDispatcher;
        _microphoneAccess = microphoneAccess;
        _shouldRun = settings.CurrentValue.ShouldRecordAudio;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(() => ExecuteInternal(cancellationToken), cancellationToken);
    }

    public Task StopAsync()
    {
        return Task.CompletedTask;
    }

    private void ExecuteInternal(CancellationToken cancellationToken)
    {
        if (!_shouldRun)
        {
            return;
        }

        foreach (var recording in _microphoneAccess.StartRecording())
        {
            if (!cancellationToken.IsCancellationRequested && _shouldRun)
            {
                _eventDispatcher.Dispatch(new AudioCapturedEvent(recording));
            }
        }

        _microphoneAccess.StopRecording();
    }

    public Task HandleAsync(SettingsUpdatedEvent<MicrophoneSettings> @event)
    {
        _shouldRun = @event.NewValue.ShouldRecordAudio;
        return Task.CompletedTask;
    }
}
