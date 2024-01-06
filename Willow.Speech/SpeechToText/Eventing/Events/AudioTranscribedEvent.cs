namespace Willow.Speech.SpeechToText.Eventing.Events;

public readonly record struct AudioTranscribedEvent(string Text);