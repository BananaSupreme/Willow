using Willow.Core.Eventing.Abstractions;
using Willow.Speech.SpeechToText.Eventing.Events;
using Willow.Speech.Tokenization.Eventing.Interceptors;

namespace Willow.Speech.SpeechToText.Registration;

internal sealed class AudioTranscribedEventInterceptorRegistrar : IInterceptorRegistrar
{
    public static void RegisterInterceptor(IEventDispatcher eventDispatcher)
    {
        eventDispatcher.RegisterInterceptor<AudioTranscribedEvent, PunctuationRemoverInterceptor>();
    }
}
