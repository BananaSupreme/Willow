using Willow.Speech.Microphone.Models;

namespace Willow.Speech.Microphone.Abstractions;

public interface IMicrophoneAccess
{
    IEnumerable<AudioData> StartRecording();
    void StopRecording();
}