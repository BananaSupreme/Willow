using Willow.Core.Eventing.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Events;
using Willow.Core.SpeechCommands.SpeechRecognition.VAD.Eventing.Interceptors;

namespace Willow.Core.SpeechCommands.SpeechRecognition.VAD.Registration;

internal class VoiceActivityDetectionInterceptorRegistrar : IInterceptorRegistrar
{
    public static void RegisterInterceptor(IEventDispatcher eventDispatcher)
    {
        eventDispatcher.RegisterInterceptor<AudioCapturedEvent, VoiceActivityDetectionInterceptor>();
    }
}