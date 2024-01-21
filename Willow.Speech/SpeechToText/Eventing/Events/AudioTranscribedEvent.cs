namespace Willow.Speech.SpeechToText.Eventing.Events;

/// <summary>
/// Input audio was transcribed and is ready to be processed into a command.
/// </summary>
/// <param name="Text">The transcribed text.</param>
public readonly record struct AudioTranscribedEvent(string Text);
