using Willow.Speech.SpeechRecognition.Microphone.Models;

namespace Willow.Speech.SpeechRecognition.SpeechToText.Abstractions;

public interface ISpeechToTextEngine
{
    public Task<string> TranscribeAudioAsync(AudioData audioData);
}