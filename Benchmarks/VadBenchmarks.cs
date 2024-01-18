using BenchmarkDotNet.Attributes;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Willow.Core.Settings.Abstractions;
using Willow.Speech.AudioBuffering;
using Willow.Speech.AudioBuffering.Settings;
using Willow.Speech.Microphone.Eventing.Events;
using Willow.Speech.Microphone.Models;
using Willow.Speech.VAD;
using Willow.Speech.VAD.Eventing.Interceptors;
using Willow.Speech.VAD.Settings;

namespace Benchmarks;

[MemoryDiagnoser]
public sealed class VadBenchmarks
{
    private AudioCapturedEvent _audioDataEvent;
    private VoiceActivityDetectionInterceptor _interceptor = null!;

    [GlobalSetup]
    public void Setup()
    {
        var filePath = Path.Combine(Environment.CurrentDirectory, "TestData/test.wav");
        var audioData = GetFromWavFile(File.ReadAllBytes(filePath));
        audioData = audioData with { RawData = audioData.RawData[..audioData.SamplingRate] };
        _audioDataEvent = new AudioCapturedEvent(audioData);
        var sileroSettings = Substitute.For<ISettings<SileroSettings>>();
        sileroSettings.CurrentValue.Returns(static _ => new SileroSettings());
        var bufferSettings = Substitute.For<ISettings<AudioBufferSettings>>();
        bufferSettings.CurrentValue.Returns(static _ => new AudioBufferSettings());
        _interceptor = new VoiceActivityDetectionInterceptor(
            new SileroVoiceActivityDetectionFacade(sileroSettings,
                                                   Substitute.For<ILogger<SileroVoiceActivityDetectionFacade>>()),
            new AudioBuffer(bufferSettings),
            Substitute.For<ILogger<VoiceActivityDetectionInterceptor>>());
    }

    [Benchmark]
    public Task Vad()
    {
        return _interceptor.InterceptAsync(_audioDataEvent, static _ => Task.CompletedTask);
    }

    private static AudioData GetFromWavFile(byte[] wav)
    {
        var samplingRate = BitConverter.ToInt32(wav, 24);
        var bitDepth = BitConverter.ToUInt16(wav, 34);
        var channelCount = BitConverter.ToUInt16(wav, 22);
        var data = new short[BitConverter.ToInt32(wav, 40) / channelCount];
        for (var i = 0; i < data.Length / 2; i++)
        {
            data[i] = BitConverter.ToInt16(wav, i * 2 * channelCount);
        }

        return new AudioData(data, samplingRate, channelCount, bitDepth);
    }
}
