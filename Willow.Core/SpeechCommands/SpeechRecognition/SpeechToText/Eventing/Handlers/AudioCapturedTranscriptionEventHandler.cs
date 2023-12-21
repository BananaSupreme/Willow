using Willow.Core.Eventing.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Events;
using Willow.Core.SpeechCommands.SpeechRecognition.SpeechToText.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.SpeechToText.Eventing.Events;

namespace Willow.Core.SpeechCommands.SpeechRecognition.SpeechToText.Eventing.Handlers;

internal sealed class AudioCapturedTranscriptionEventHandler : IEventHandler<AudioCapturedEvent>
{
    private readonly IEventDispatcher _eventDispatcher;
    private readonly ISpeechToTextEngine _speechToTextEngine;

    public AudioCapturedTranscriptionEventHandler(ISpeechToTextEngine speechToTextEngine,
                                                  IEventDispatcher eventDispatcher)
    {
        _speechToTextEngine = speechToTextEngine;
        _eventDispatcher = eventDispatcher;
    }

    public async Task HandleAsync(AudioCapturedEvent @event)
    {
        var text = await _speechToTextEngine.TranscribeAudioAsync(@event.AudioData);
        _eventDispatcher.Dispatch(new AudioTranscribedEvent(text));
    }
}