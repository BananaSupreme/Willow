using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Models;
using Willow.WhisperServer;
using Willow.WhisperServer.Enum;
using Willow.WhisperServer.Models;
using Willow.WhisperServer.Settings;

namespace Tests.VoiceCommands.Whisper;

public class WhisperEngineTests
{
    [Fact]
    public void Sanity()
    {
        var services = new ServiceCollection();
        services.AddSingleton<WhisperEngine>();
        services.AddLogging();
        services.Configure<PythonSettings>(options =>
            options.PathToPythonDll = @"C:\Users\Supremus\AppData\Local\Programs\Python\Python311\python311.dll");
        services.Configure<WhisperModelSettings>(options =>
        {
            options.ModelSize = ModelSize.Tiny;
            options.EnglishOnly = true;
            options.ComputeType = ComputeType.Int8;
            options.Device = DeviceType.Cpu;
        });
        services.Configure<TranscriptionSettings>(_ =>
        {
            /* set default values */
        });
        var serviceProvider = services.BuildServiceProvider();

        var filePath = Path.Combine(Environment.CurrentDirectory, "VoiceCommands/Whisper/test.wav");
        var audioData = File.ReadAllBytes(filePath);

        var transcriptionParameters = new TranscriptionParameters(GetFromWavFile(audioData));
        var whisperEngine = serviceProvider.GetRequiredService<WhisperEngine>();
        var transcriptionResult = whisperEngine.Transcribe(transcriptionParameters);
        transcriptionResult.Should().Be("I think I will cry.");
        whisperEngine.Dispose();
        transcriptionResult.Should().Be("I think I will cry.");
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

        return new(data, samplingRate, channelCount, bitDepth);
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