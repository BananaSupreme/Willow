using Microsoft.Extensions.Hosting;

using Willow.Core.Eventing.Abstractions;
using Willow.Speech.Microphone.Abstractions;
using Willow.Speech.Microphone.Eventing.Events;

namespace Willow.Speech.Microphone;

internal sealed class MicrophoneWorker : BackgroundService
{
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IMicrophoneAccess _microphoneAccess;

    public MicrophoneWorker(IEventDispatcher eventDispatcher,
                            IMicrophoneAccess microphoneAccess)
    {
        _eventDispatcher = eventDispatcher;
        _microphoneAccess = microphoneAccess;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(() => ExecuteInternal(stoppingToken), stoppingToken);
    }

    private void ExecuteInternal(CancellationToken stoppingToken)
    {
        foreach (var recording in _microphoneAccess.StartRecording())
        {
            if (!stoppingToken.IsCancellationRequested)
            {
                _eventDispatcher.Dispatch(new AudioCapturedEvent(recording));
            }
            else
            {
                break;
            }
        }

        _microphoneAccess.StopRecording();
    }
}