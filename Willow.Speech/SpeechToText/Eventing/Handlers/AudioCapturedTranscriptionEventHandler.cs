using Willow.Core.Eventing.Abstractions;
using Willow.Speech.Microphone.Eventing.Events;
using Willow.Speech.SpeechToText.Abstractions;
using Willow.Speech.SpeechToText.Eventing.Events;

namespace Willow.Speech.SpeechToText.Eventing.Handlers;

internal sealed class AudioCapturedTranscriptionEventHandler : IEventHandler<AudioCapturedEvent>
{
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IEnumerable<ISpeechToTextEngine> _speechToTextEngines;

    public AudioCapturedTranscriptionEventHandler(IEnumerable<ISpeechToTextEngine> speechToTextEngines,
                                                  IEventDispatcher eventDispatcher)
    {
        _speechToTextEngines = speechToTextEngines;
        _eventDispatcher = eventDispatcher;
    }

    public async Task HandleAsync(AudioCapturedEvent @event)
    {
        var speechToTextEngine = _speechToTextEngines.FirstOrDefault(x => x.IsRunning);
        if (speechToTextEngine is null)
        {
            return;
        }

        var text = await speechToTextEngine.TranscribeAudioAsync(@event.AudioData);
        _eventDispatcher.Dispatch(new AudioTranscribedEvent(text));
    }
}