using Willow.Speech.Microphone.Models;

namespace Willow.Speech.Resampling.Abstractions;
/// <summary>
/// Resampler to convert the sample of the <see cref="AudioData"/>.
/// </summary>
internal interface IResampler
{
    /// <summary>
    /// Resamples incoming data from its own sample rate into the requested sampling rate.
    /// </summary>
    /// <param name="input">Input data.</param>
    /// <param name="requestedSamplingRate">Requested data.</param>
    /// <returns>New data resampled to the requested sampling rate.</returns>
    AudioData Resample(AudioData input, int requestedSamplingRate);
}