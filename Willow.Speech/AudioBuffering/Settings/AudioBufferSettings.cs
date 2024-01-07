namespace Willow.Speech.AudioBuffering.Settings;

public readonly record struct AudioBufferSettings(
    int AcceptedSamplingRate,
    int MaxSeconds)
{
    public AudioBufferSettings()
        : this(16000, 10)
    {
    }
}