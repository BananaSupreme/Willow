namespace Willow.Speech.AudioBuffering.Settings;

[ToString]
public sealed class AudioBufferSettings
{
    public int AcceptedSamplingRate { get; set; } = 16000;
    public int MaxSeconds { get; set; } = 10;
}