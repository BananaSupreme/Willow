using Willow.Speech.Microphone.Models;

namespace Willow.Speech.AudioBuffering.Exceptions;

/// <summary>
/// Thrown when adding a new sample into the audio buffer but the sample features, such as sampling rate or channel
/// buffer are different from the already included input. <br/>
/// The system throws this exception as a sample when there is a different sampling rate, or a different channel count
/// is not the same as another sample with different parameters.
/// </summary>
public sealed class MismatchedFeaturesException : ArgumentException
{
    public MismatchedFeaturesException(AudioData audioData,
                                       int samplingRateInBuffer,
                                       ushort channelCountInBuffer,
                                       ushort bitDepthInBuffer) : this(
        $"Mismatch in audio features detected. The provided audio data has a sampling rate of {audioData.SamplingRate} Hz, channel count of {audioData.ChannelCount}, and bit depth of {audioData.BitDepth} bits, which does not match the expected sampling rate of {samplingRateInBuffer} Hz, channel count of {channelCountInBuffer}, and bit depth of {bitDepthInBuffer} bits. Please ensure that the audio data matches the expected format.")

    {
    }

    public MismatchedFeaturesException()
    {
    }

    public MismatchedFeaturesException(string? message) : base(message)
    {
    }

    public MismatchedFeaturesException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public MismatchedFeaturesException(string? message, string? paramName) : base(message, paramName)
    {
    }

    public MismatchedFeaturesException(string? message, string? paramName, Exception? innerException) : base(message,
        paramName,
        innerException)
    {
    }
}
