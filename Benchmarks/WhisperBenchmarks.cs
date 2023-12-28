using BenchmarkDotNet.Attributes;

using DryIoc.Microsoft.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Models;
using Willow.Core.SpeechCommands.SpeechRecognition.SpeechToText.Abstractions;
using Willow.WhisperServer;

namespace Benchmarks;

[MemoryDiagnoser]
public class WhisperBenchmarks
{
    private AudioData _audioData;
    private WhisperEngine _pythonWhisperEngine = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        WillowWhisperServerStartup.Register(services, new ConfigurationManager());

        var serviceProvider = services.CreateServiceProvider();

        var filePath = Path.Combine(Environment.CurrentDirectory, "TestData/test.wav");

        _audioData = GetFromWavFile(await File.ReadAllBytesAsync(filePath));
        _pythonWhisperEngine = serviceProvider
                               .GetRequiredService<IEnumerable<ISpeechToTextEngine>>()
                               .OfType<WhisperEngine>()
                               .First();

        await _pythonWhisperEngine.StartAsync(CancellationToken.None);
        var transcribe = await _pythonWhisperEngine.TranscribeAudioAsync(_audioData);
        Console.WriteLine(transcribe);
    }

    [Benchmark]
    public async Task TranscribePython()
    {
        _ = await _pythonWhisperEngine.TranscribeAudioAsync(_audioData);
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _pythonWhisperEngine.StopAsync(CancellationToken.None);
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