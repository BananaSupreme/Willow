namespace Willow.Speech.SpeechRecognition.SpeechToText.Eventing.Events;

public readonly record struct AudioTranscribedEvent(string Text);