using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Models;

namespace Willow.Core.SpeechCommands.SpeechRecognition.Resampling.Abstractions;

public interface IResampler
{
    AudioData Resample(AudioData input, int requestedSamplingRate);
}