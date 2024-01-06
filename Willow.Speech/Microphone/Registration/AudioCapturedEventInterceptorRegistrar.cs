using Willow.Core.Eventing.Abstractions;
using Willow.Speech.Microphone.Eventing.Events;
using Willow.Speech.Microphone.Eventing.Interceptors;
using Willow.Speech.Resampling.Eventing.Interceptors;
using Willow.Speech.VAD.Eventing.Interceptors;

namespace Willow.Speech.Microphone.Registration;

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