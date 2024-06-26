using Tests.Helpers;

using Willow.Eventing.Registration;
using Willow.Speech.Microphone.Models;
using Willow.Speech.SpeechToText;
using Willow.WhisperServer;

using Xunit.Abstractions;

namespace Tests.Speech.SpeechServers;

public sealed class WhisperEngineTests : IAsyncLifetime
{
    private const string Expected = "I think I will cry";
    private readonly ServiceProvider _serviceProvider;
    private AudioData _audioData;

    public WhisperEngineTests(ITestOutputHelper testOutputHelper)
    {
        var services = new ServiceCollection();
        new EventingRegistrar().RegisterServices(services);
        services.AddSettings();
        new WillowWhisperServerRegistrar().RegisterServices(services);
        services.AddTestLogger(testOutputHelper);
        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task InitializeAsync()
    {
        var filePath = Path.Combine(Environment.CurrentDirectory, "Speech", "SpeechServers", "test.wav");
        var audioData = await File.ReadAllBytesAsync(filePath);
        _audioData = GetFromWavFile(audioData);
    }

    public async Task DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
    }

    [Fact]
    public async Task When_ServerStartedAndStopped_StartsAgainOK()
    {
        var whisperEngine = (WhisperEngine)_serviceProvider.GetRequiredService<ISpeechToTextEngine>();
        await TestInternal(whisperEngine);
        await TestInternal(whisperEngine);
    }

    private async Task TestInternal(WhisperEngine whisperEngine)
    {
        await whisperEngine.StartAsync(CancellationToken.None);
        var transcriptionResult = await whisperEngine.TranscribeAudioAsync(_audioData);
        transcriptionResult[..Expected.Length].Should().Be(Expected);
        await whisperEngine.StopAsync();
        transcriptionResult[..Expected.Length].Should().Be(Expected);
    }

    private static AudioData GetFromWavFile(byte[] wav)
    {
        var samplingRate = BitConverter.ToInt32(wav, 24);
        var bitDepth = BitConverter.ToUInt16(wav, 34);
        var channelCount = BitConverter.ToUInt16(wav, 22);
        var data = new short[BitConverter.ToInt32(wav, 40) / 2];
        for (var i = 0; i < data.Length; i++)
        {
            data[i] = BitConverter.ToInt16(wav, i * 2);
        }

        return new AudioData(data, samplingRate, channelCount, bitDepth);
    }
}
/*
WAV STANDARD
0-4 RIFF
4-8 File Size
8-12 WAVE
12-16 fmt
16-20 16 as int
20-22 1 as short
22-24 channel count as short
24-28 sample rate as int
28-32 byte rate as int
32-34 block align as short
34-36 bit depth as short
36-40 data
44-48 data size
*/
