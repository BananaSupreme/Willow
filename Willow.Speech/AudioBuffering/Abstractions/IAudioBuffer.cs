using Willow.Speech.Microphone.Models;

namespace Willow.Speech.AudioBuffering.Abstractions;

public interface IAudioBuffer
{
    bool HasSpace(int space);
    bool TryLoadData(AudioData audioData);
    AudioData UnloadAllData();
    (AudioData AudioData, int ActualSize) UnloadData(int maximumRequested);
}