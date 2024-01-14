namespace Willow.Speech.VAD.Settings;

/// <summary>
/// this input arguments into the voice activity detector backed by Silero-VAD.
/// </summary>
/// <param name="Threshold">
/// Speech threshold. Silero VAD outputs speech probabilities for each audio chunk,
/// probabilities ABOVE this value are considered as SPEECH. It is better to tune this
/// parameter for each dataset separately, but "lazy" 0.5 is pretty good for most datasets.
/// </param>
/// <param name="MinSpeechDurationMilliseconds">
/// Final speech chunks shorter min_speech_duration_ms are thrown out.
/// </param>
/// <param name="SpeechPadMilliseconds">
/// Final speech chunks are padded by speech_pad_ms each side.
/// </param>
/// <seealso href="https://github.com/snakers4/silero-vad"/>
public readonly record struct SileroSettings(
    float Threshold,
    int MinSpeechDurationMilliseconds,
    int SpeechPadMilliseconds)
{
    /// <inheritdoc cref="SileroSettings"/>
    public SileroSettings()
        : this(0.5f,
            10,
            100)
    {
    }
}