using Willow.Core.Eventing.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.SpeechToText.Eventing.Events;
using Willow.Core.SpeechCommands.Tokenization.Eventing.Interceptors;

namespace Willow.Core.SpeechCommands.SpeechRecognition.SpeechToText.Registration;

internal sealed class AudioTranscribedEventInterceptorRegistrar : IInterceptorRegistrar
{
    public static void RegisterInterceptor(IEventDispatcher eventDispatcher)
    {
        eventDispatcher.RegisterInterceptor<AudioTranscribedEvent, PunctuationRemoverInterceptor>();
    }
}