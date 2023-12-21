using Willow.Core.Eventing.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Events;

namespace Willow.Core.SpeechCommands.SpeechRecognition.VAD.Eventing.Interceptors;

internal class VoiceActivityDetectionInterceptorRegistrar : IInterceptorRegistrar
{
    public static void RegisterInterceptor(IEventDispatcher eventDispatcher)
    {
        eventDispatcher.RegisterInterceptor<AudioCapturedEvent, VoiceActivityDetectionInterceptor>();
    }
}