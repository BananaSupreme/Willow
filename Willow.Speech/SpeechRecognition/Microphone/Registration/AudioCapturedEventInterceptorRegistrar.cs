using Willow.Core.Eventing.Abstractions;
using Willow.Speech.SpeechRecognition.Microphone.Eventing.Events;
using Willow.Speech.SpeechRecognition.Resampling.Eventing.Interceptors;
using Willow.Speech.SpeechRecognition.VAD.Eventing.Interceptors;

#if DEBUG
using Willow.Speech.SpeechRecognition.Microphone.Eventing.Interceptors;
#endif

namespace Willow.Speech.SpeechRecognition.Microphone.Registration;

internal sealed class AudioCapturedEventInterceptorRegistrar : IInterceptorRegistrar
{
    public static void RegisterInterceptor(IEventDispatcher eventDispatcher)
    {
        eventDispatcher.RegisterInterceptor<AudioCapturedEvent, ResamplingInterceptor>();
        eventDispatcher.RegisterInterceptor<AudioCapturedEvent, VoiceActivityDetectionInterceptor>();
#if DEBUG
        eventDispatcher.RegisterInterceptor<AudioCapturedEvent, DebuggingVoiceWavOutputInterceptor>();
#endif
    }
}