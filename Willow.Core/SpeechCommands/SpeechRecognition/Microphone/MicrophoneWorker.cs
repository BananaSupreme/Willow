using Microsoft.Extensions.Hosting;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Events;

namespace Willow.Core.SpeechCommands.SpeechRecognition.Microphone;

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

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
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
        return Task.CompletedTask;
    }
}