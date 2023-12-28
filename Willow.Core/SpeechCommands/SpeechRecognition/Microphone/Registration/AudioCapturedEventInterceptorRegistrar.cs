using Willow.Core.Eventing.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Events;
using Willow.Core.SpeechCommands.SpeechRecognition.Resampling.Eventing.Interceptors;
using Willow.Core.SpeechCommands.SpeechRecognition.VAD.Eventing.Interceptors;

namespace Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Registration;

internal sealed class AudioCapturedEventInterceptorRegistrar : IInterceptorRegistrar
{
    public static void RegisterInterceptor(IEventDispatcher eventDispatcher)
    {
        eventDispatcher.RegisterInterceptor<AudioCapturedEvent, ResamplingInterceptor>();
        eventDispatcher.RegisterInterceptor<AudioCapturedEvent, VoiceActivityDetectionInterceptor>();
    }
}