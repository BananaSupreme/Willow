using Willow.Speech.Microphone.Models;

namespace Willow.Speech.Resampling.Abstractions;

public interface IResampler
{
    AudioData Resample(AudioData input, int requestedSamplingRate);
}