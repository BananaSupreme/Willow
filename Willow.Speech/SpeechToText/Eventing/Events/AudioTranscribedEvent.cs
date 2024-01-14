using Willow.Speech.SpeechToText.Registration;

namespace Willow.Speech.SpeechToText.Eventing.Events;

/// <summary>
/// Input audio was transcribed and is ready to be processed into a command.
/// </summary>
/// <remarks>
/// Interceptors should be registered at <see cref="AudioTranscribedEventInterceptorRegistrar"/>.
/// </remarks>
/// <param name="Text">The transcribed text.</param>
public readonly record struct AudioTranscribedEvent(string Text);