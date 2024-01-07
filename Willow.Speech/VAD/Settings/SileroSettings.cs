namespace Willow.Speech.VAD.Settings;

public readonly record struct SileroSettings(
    float Threshold,
    int MinSpeechDurationMilliseconds,
    float MaxSpeechDurationSeconds,
    int MinSilenceDurationMilliseconds,
    int WindowSizeSamples,
    int SpeechPadMilliseconds)
{
    public SileroSettings()
        : this(0.5f,
            10,
            float.PositiveInfinity,
            10,
            1536,
            100)
    {
    }
}