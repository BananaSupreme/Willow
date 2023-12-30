using Willow.Speech.SpeechRecognition.Microphone.Models;

namespace Willow.Speech.SpeechRecognition.AudioBuffering.Abstractions;

public interface IAudioBuffer
{
    bool HasSpace(int space);
    bool TryLoadData(AudioData audioData);
    AudioData UnloadAllData();
    (AudioData AudioData, int ActualSize) UnloadData(int maximumRequested);
}