using Willow.Eventing;
using Willow.Registration;
using Willow.Speech.Microphone.Abstractions;
using Willow.Speech.Microphone.Events;

namespace Willow.Speech.Microphone;

internal sealed class MicrophoneWorker : IBackgroundWorker
{
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IMicrophoneAccess _microphoneAccess;

    public MicrophoneWorker(IEventDispatcher eventDispatcher, IMicrophoneAccess microphoneAccess)
    {
        _eventDispatcher = eventDispatcher;
        _microphoneAccess = microphoneAccess;
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
        foreach (var recording in _microphoneAccess.StartRecording())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            _eventDispatcher.Dispatch(new AudioCapturedEvent(recording));
        }

        _microphoneAccess.StopRecording();
    }
}
