using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Models;

namespace Willow.Core.SpeechCommands.SpeechRecognition.AudioBuffering.Abstractions;

public interface IAudioBuffer
{
    bool HasSpace(int space);
    bool TryLoadData(AudioData audioData);
    AudioData UnloadAllData();
    (AudioData AudioData, int ActualSize) UnloadData(int maximumRequested);
}