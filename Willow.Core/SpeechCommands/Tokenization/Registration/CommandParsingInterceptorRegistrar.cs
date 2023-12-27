using Willow.Core.Eventing.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.SpeechToText.Eventing.Events;
using Willow.Core.SpeechCommands.Tokenization.Eventing.Interceptors;

namespace Willow.Core.SpeechCommands.Tokenization.Registration;

internal class CommandParsingInterceptorRegistrar : IInterceptorRegistrar
{
    public static void RegisterInterceptor(IEventDispatcher eventDispatcher)
    {
        eventDispatcher.RegisterInterceptor<AudioTranscribedEvent, PunctuationRemoverInterceptor>();
    }
}