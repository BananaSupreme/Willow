using Willow.Speech.SpeechRecognition.Microphone.Models;

namespace Willow.Speech.SpeechRecognition.Resampling.Abstractions;

public interface IResampler
{
    AudioData Resample(AudioData input, int requestedSamplingRate);
}