namespace Willow.Speech.Microphone.Models;

/// <summary>
/// A collection of samples with certain parameters.
/// </summary>
/// <param name="RawData">The samples.</param>
/// <param name="SamplingRate">The sampling rate the samples are sampled in.</param>
/// <param name="ChannelCount">The amount of channels the samples are in.</param>
/// <param name="BitDepth">The bit depth of the samples.</param>
public readonly record struct AudioData(short[] RawData,
                                        int SamplingRate,
                                        ushort ChannelCount,
                                        ushort BitDepth)
{
    public static AudioData Empty { get; } = new([], 0, 0, 0);

    /// <summary>
    /// Gets the length of time in this audio sample.
    /// </summary>
    public TimeSpan Duration => FromSamplePosition(RawData.Length);

    /// <summary>
    /// Returns a normalized float array of data with values between 1.0 and 0.0.
    /// </summary>
    public Lazy<float[]> NormalizedData => new(Normalize());

    /// <summary>
    /// Returns the duration of audio that at the relevant <paramref name="position" />.
    /// </summary>
    /// <param name="position">The position to know the timestamp of the audio at.</param>
    /// <returns>The time at the position.</returns>
    public TimeSpan FromSamplePosition(int position)
    {
        return this != Empty ? TimeSpan.FromSeconds((double)position / SamplingRate) : TimeSpan.Zero;
    }

    /// <summary>
    /// Returns the sample position at the incoming <paramref cref="time" />.
    /// </summary>
    /// <param name="time">The requested time.</param>
    /// <returns>The position at this time.</returns>
    public int FromTimeSpan(TimeSpan time)
    {
        return (int)(SamplingRate * time.TotalSeconds);
    }

    private float[] Normalize()
    {
        return RawData.Select(static x => x / (float)32768.0).ToArray();
    }

    public override string ToString()
    {
        return
            $"RawData Length: {RawData.Length}, Audio Duration: {Duration}, SamplingRate: {SamplingRate}, ChannelCount: {ChannelCount}, BitDepth: {BitDepth}";
    }
}
