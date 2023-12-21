using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Models;

namespace Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Abstractions;

public interface IMicrophoneAccess
{
    IEnumerable<AudioData> StartRecording();
    void StopRecording();
}