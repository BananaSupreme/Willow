using Willow.Speech.Microphone.Models;

namespace Willow.Speech.SpeechToText.Abstractions;

public interface ISpeechToTextEngine
{
    public Task<string> TranscribeAudioAsync(AudioData audioData);
}