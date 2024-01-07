using Willow.Core.Environment.Enums;
using Willow.Speech.Microphone.Models;

namespace Willow.Speech.SpeechToText.Abstractions;

public interface ISpeechToTextEngine
{
    public string Name { get; }
    public bool IsRunning { get; }
    public SupportedOperatingSystems SupportedOperatingSystems { get; }
    public Task<string> TranscribeAudioAsync(AudioData audioData);
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}