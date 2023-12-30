using Willow.Speech.SpeechRecognition.Microphone.Models;

namespace Willow.Speech.SpeechRecognition.Microphone.Abstractions;

public interface IMicrophoneAccess
{
    IEnumerable<AudioData> StartRecording();
    void StopRecording();
}