using Willow.Eventing;
using Willow.Middleware;
using Willow.Registration;
using Willow.Speech.Microphone.Events;
using Willow.Speech.Microphone.Middleware;
using Willow.Speech.Resampling.Middleware;
using Willow.Speech.SpeechToText.Events;
using Willow.Speech.VAD.Middleware;

namespace Willow.Speech.SpeechToText.EventHandlers;

/// <summary>
/// Handles the audio after the entire processing pipeline and uses the selected STT engine to transcribe the message.
/// </summary>
internal sealed class AudioCapturedTranscriptionEventHandler : IEventHandler<AudioCapturedEvent>
{
    private readonly IEventDispatcher _eventDispatcher;
    private readonly ICollectionProvider<ISpeechToTextEngine> _speechToTextEngines;
    private readonly IMiddlewarePipeline<AudioCapturedEvent> _pipeline;

    public AudioCapturedTranscriptionEventHandler(ICollectionProvider<ISpeechToTextEngine> speechToTextEngines,
                                                  IMiddlewareBuilderFactory<AudioCapturedEvent> middlewareBuilderFactory,
                                                  ResamplingMiddleware resamplingMiddleware,
                                                  VoiceActivityDetectionMiddleware voiceActivityDetectionMiddleware,
#if DEBUG
                                                  DebuggingVoiceWavOutputMiddleware debuggingVoiceWavOutputMiddleware,
#endif
                                                  IEventDispatcher eventDispatcher)
    {
        _speechToTextEngines = speechToTextEngines;
        _eventDispatcher = eventDispatcher;
        _pipeline = middlewareBuilderFactory.Create()
                                            .Add(resamplingMiddleware)
                                            .Add(voiceActivityDetectionMiddleware)
#if DEBUG
                                            .Add(debuggingVoiceWavOutputMiddleware)
#endif
                                            .Build();
    }

    public async Task HandleAsync(AudioCapturedEvent @event)
    {
        await _pipeline.ExecuteAsync(@event, HandleCoreAsync);
    }

    public async Task HandleCoreAsync(AudioCapturedEvent @event)
    {
        var speechToTextEngine = _speechToTextEngines.Get().FirstOrDefault(static x => x.IsRunning);
        if (speechToTextEngine is null)
        {
            return;
        }

        var text = await speechToTextEngine.TranscribeAudioAsync(@event.AudioData);
        _eventDispatcher.Dispatch(new AudioTranscribedEvent(text));
    }
}
