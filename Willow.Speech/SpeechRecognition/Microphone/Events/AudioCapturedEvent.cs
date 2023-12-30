using Willow.Speech.SpeechRecognition.Microphone.Models;

namespace Willow.Speech.SpeechRecognition.Microphone.Events;

public readonly record struct AudioCapturedEvent(AudioData AudioData);