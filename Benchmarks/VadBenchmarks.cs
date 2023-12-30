using BenchmarkDotNet.Attributes;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NSubstitute;

using Willow.Speech.SpeechRecognition.AudioBuffering;
using Willow.Speech.SpeechRecognition.AudioBuffering.Settings;
using Willow.Speech.SpeechRecognition.Microphone.Events;
using Willow.Speech.SpeechRecognition.Microphone.Models;
using Willow.Speech.SpeechRecognition.VAD;
using Willow.Speech.SpeechRecognition.VAD.Eventing.Interceptors;
using Willow.Speech.SpeechRecognition.VAD.Settings;

namespace Benchmarks;

[MemoryDiagnoser]
public class VadBenchmarks
{
    private AudioCapturedEvent _audioDataEvent;
    private VoiceActivityDetectionInterceptor _interceptor = null!;

    [GlobalSetup]
    public void Setup()
    {
        var filePath = Path.Combine(Environment.CurrentDirectory, "TestData/test.wav");
        var audioData = GetFromWavFile(File.ReadAllBytes(filePath));
        audioData = audioData with { RawData = audioData.RawData[..audioData.SamplingRate] };
        _audioDataEvent = new(audioData);
        var sileroSettings = Substitute.For<IOptionsMonitor<SileroSettings>>();
        sileroSettings.CurrentValue.Returns(_ => new());
        var bufferSettings = Substitute.For<IOptionsMonitor<AudioBufferSettings>>();
        bufferSettings.CurrentValue.Returns(_ => new());
        _interceptor = new(
            new SileroVoiceActivityDetectionFacade(sileroSettings,
                Substitute.For<ILogger<SileroVoiceActivityDetectionFacade>>()),
            new AudioBuffer(bufferSettings),
            Substitute.For<ILogger<VoiceActivityDetectionInterceptor>>());
    }

    [Benchmark]
    public Task Vad()
    {
        return _interceptor.InterceptAsync(_audioDataEvent, _ => Task.CompletedTask);
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

        return new(data, samplingRate, channelCount, bitDepth);
    }
}