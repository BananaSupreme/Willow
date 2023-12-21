using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Models;
using Willow.Core.SpeechCommands.SpeechRecognition.VAD.Models;

namespace Willow.Core.SpeechCommands.SpeechRecognition.VAD.Abstractions;

public interface IVoiceActivityDetection
{
    VoiceActivityResult Detect(AudioData audioSegment);
}