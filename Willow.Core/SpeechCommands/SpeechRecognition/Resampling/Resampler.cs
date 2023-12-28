using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Models;
using Willow.Core.SpeechCommands.SpeechRecognition.Resampling.Abstractions;

namespace Willow.Core.SpeechCommands.SpeechRecognition.Resampling;

internal sealed class Resampler : IResampler
{
    public AudioData Resample(AudioData input, int requestedSamplingRate)
    {
        if (input.SamplingRate == requestedSamplingRate)
        {
            return input;
        }
        else if (input.SamplingRate < requestedSamplingRate)
        {
            return DownSample(input, requestedSamplingRate);
        }
        else
        {
            return UpSample(input, requestedSamplingRate);
        }
    }

    private AudioData DownSample(AudioData input, int requestedSamplingRate)
    {
        var factor = GetFactor(input.SamplingRate, requestedSamplingRate);
        var resampled = new short[(int)(input.RawData.Length / factor)];
        for (var i = 0; i < resampled.Length; i++)
        {
            var inputEquivalent = (int)Math.Round(i * factor);
            resampled[i] = input.RawData[inputEquivalent];
        }

        return input with { RawData = resampled, SamplingRate = requestedSamplingRate };
    }

    private AudioData UpSample(AudioData input, int requestedSamplingRate)
    {
        var factor = GetFactor(input.SamplingRate, requestedSamplingRate);
        var resampled = new short[(int)(input.RawData.Length * factor)];
        for (var i = 0; i < resampled.Length; i++)
        {
            var ceilingIndex = (int)Math.Ceiling(i * factor);
            var floorIndex = Math.Min(input.RawData.Length, (int)Math.Floor(i * factor));
            var inputEquivalent = (int)Math.Round((input.RawData[ceilingIndex] + input.RawData[floorIndex]) / 2.0);
            resampled[i] = input.RawData[inputEquivalent];
        }

        return input with { RawData = resampled, SamplingRate = requestedSamplingRate };
    }

    private float GetFactor(int inputSamplingRate, int requestedSamplingRate)
    {
        return inputSamplingRate / (float)requestedSamplingRate;
    }
}