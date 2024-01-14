namespace Willow.Speech.AudioBuffering.Settings;

/// <summary>
/// The parameters of the audio buffer.
/// </summary>
/// <param name="AcceptedSamplingRate">The expected sample rate, default 16000.</param>
/// <param name="MaxSeconds">
/// The maximum size of the buffer in seconds, default 10 seconds (or 160000 samples at 16Khz).
/// </param>
public readonly record struct AudioBufferSettings(
    int AcceptedSamplingRate,
    int MaxSeconds)
{
    /// <inheritdoc cref="AudioBufferSettings"/>
    public AudioBufferSettings()
        : this(16000, 10)
    {
    }
}