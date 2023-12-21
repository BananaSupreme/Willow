namespace Willow.Core.SpeechCommands.SpeechRecognition.SpeechToText.Eventing.Events;

public readonly record struct AudioTranscribedEvent(string Text);