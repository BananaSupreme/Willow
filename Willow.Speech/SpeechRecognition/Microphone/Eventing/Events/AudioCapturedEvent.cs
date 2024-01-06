using Willow.Speech.SpeechRecognition.Microphone.Models;

namespace Willow.Speech.SpeechRecognition.Microphone.Eventing.Events;

public readonly record struct AudioCapturedEvent(AudioData AudioData);