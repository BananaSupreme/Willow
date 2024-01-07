using Willow.Speech.Microphone.Models;

namespace Willow.Speech.SpeechToText.Abstractions;

public interface ISpeechToTextEngine
{
    public Task<string> TranscribeAudioAsync(AudioData audioData);
    public string Name { get; }
    public bool IsRunning { get; }
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}