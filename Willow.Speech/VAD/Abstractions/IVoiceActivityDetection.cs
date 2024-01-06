using Willow.Speech.Microphone.Models;
using Willow.Speech.VAD.Models;

namespace Willow.Speech.VAD.Abstractions;

public interface IVoiceActivityDetection
{
    VoiceActivityResult Detect(AudioData audioSegment);
}