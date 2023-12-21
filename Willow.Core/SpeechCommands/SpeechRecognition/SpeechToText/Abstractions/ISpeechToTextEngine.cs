using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Models;

namespace Willow.Core.SpeechCommands.SpeechRecognition.SpeechToText.Abstractions;

public interface ISpeechToTextEngine
{
    public Task<string> TranscribeAudioAsync(AudioData audioData);
}