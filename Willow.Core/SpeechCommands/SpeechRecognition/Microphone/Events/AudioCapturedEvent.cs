using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Models;

namespace Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Events;

public readonly record struct AudioCapturedEvent(AudioData AudioData);