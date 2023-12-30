using Willow.Speech.SpeechRecognition.Microphone.Models;
using Willow.Speech.SpeechRecognition.VAD.Models;

namespace Willow.Speech.SpeechRecognition.VAD.Abstractions;

public interface IVoiceActivityDetection
{
    VoiceActivityResult Detect(AudioData audioSegment);
}