using Willow.Core.Eventing.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.SpeechToText.Eventing.Events;

namespace Willow.Core.SpeechCommands.Tokenization.Eventing.Interceptors;

internal class CommandParsingInterceptorRegistrar : IInterceptorRegistrar
{
    public static void RegisterInterceptor(IEventDispatcher eventDispatcher)
    {
        eventDispatcher.RegisterInterceptor<AudioTranscribedEvent, PunctuationRemoverInterceptor>();
    }
}