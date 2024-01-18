using Tests.Helpers;

using Willow.Core.Eventing.Registration;
using Willow.Speech.Microphone.Models;
using Willow.Vosk;
using Willow.Vosk.Abstractions;
using Willow.Vosk.Enums;
using Willow.Vosk.Registration;
using Willow.Vosk.Settings;

namespace Tests.Speech.SpeechServers;

[Collection("vosk")]
public sealed class VoskEngineTests : IAsyncLifetime
{
    private const string Expected = "i think i will cry";
    private readonly ServiceProvider _serviceProvider;
    private AudioData _audioData;

    public VoskEngineTests()
    {
        var downloader = Substitute.For<IVoskModelDownloader>();
        var services = new ServiceCollection();
        EventingRegistrar.RegisterServices(services);
        services.AddSettings();
        services.AddLogging();
        VoskServerRegistrar.RegisterServices(services);
        services.AddSingleton(downloader);
        _serviceProvider = services.BuildServiceProvider();
        downloader.GetVoskModelZip(Arg.Any<VoskModel>())
                  .Returns(static _ => Task.FromResult(
                               (Stream)File.Open("Speech/SpeechServers/vosk-model-small-en-us-0.15.zip",
                                                 FileMode.Open)));
    }

    public async Task InitializeAsync()
    {
        var filePath = Path.Combine(Environment.CurrentDirectory, "Speech/SpeechServers/test.wav");
        var audioData = await File.ReadAllBytesAsync(filePath);
        _audioData = GetFromWavFile(audioData);
    }

    public async Task DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
        EnsureDeletedFolder();
    }

    [Fact]
    public async Task When_ServerStartedAndStopped_StartsAgainOK()
    {
        var voskEngine = _serviceProvider.GetRequiredService<VoskEngine>();
        await TestInternal(voskEngine);
        await TestInternal(voskEngine);
    }

    private async Task TestInternal(VoskEngine whisperEngine)
    {
        await whisperEngine.StartAsync(CancellationToken.None);
        var transcriptionResult = await whisperEngine.TranscribeAudioAsync(_audioData);
        transcriptionResult.Should().Be(Expected);
        await whisperEngine.StopAsync(CancellationToken.None);
        transcriptionResult.Should().Be(Expected);
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

    private void EnsureDeletedFolder()
    {
        if (Directory.Exists(VoskSettings.VoskFolder))
        {
            Directory.Delete(VoskSettings.VoskFolder, true);
        }
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
