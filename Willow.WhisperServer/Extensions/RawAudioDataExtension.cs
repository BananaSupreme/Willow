using Willow.Speech.SpeechRecognition.Microphone.Models;

namespace Willow.WhisperServer.Extensions;

internal static class RawAudioDataExtension
{
    private const int _subchunk1Size = 16; //Standard
    private const ushort _pcmAudioFormat = 1; //Standard
    private const int _headerSize = 36; //Standard
    private static readonly byte[] _riffHeader = "RIFF"u8.ToArray();
    private static readonly byte[] _waveHeader = "WAVE"u8.ToArray();
    private static readonly byte[] _fmtHeader = "fmt "u8.ToArray();
    private static readonly byte[] _dataHeader = "data"u8.ToArray();

    public static byte[] ToWavFile(this AudioData audioData)
    {
        var sampleCount = audioData.RawData.Length;
        var byteLength = sampleCount * 2; //2 bytes per audio sample
        using var memoryStream = new MemoryStream(new byte[byteLength + 44]);
        using var writer = new BinaryWriter(memoryStream);
        var byteRate = audioData.SamplingRate * audioData.ChannelCount * audioData.BitDepth / 8;
        var blockAlign = audioData.ChannelCount * audioData.BitDepth / 8;
        var subchunk2Size = sampleCount * audioData.ChannelCount * audioData.BitDepth / 8;

        // RIFF Header
        writer.Seek(0, SeekOrigin.Begin);
        writer.Write(_riffHeader);
        writer.Write(_headerSize + subchunk2Size);
        writer.Write(_waveHeader);

        // fmt Sub-chunk
        writer.Write(_fmtHeader);
        writer.Write(_subchunk1Size);
        writer.Write(_pcmAudioFormat);
        writer.Write(audioData.ChannelCount);
        writer.Write(audioData.SamplingRate);
        writer.Write(byteRate);
        writer.Write((short)blockAlign);
        writer.Write(audioData.BitDepth);

        // data Sub-chunk
        writer.Write(_dataHeader);
        writer.Write(subchunk2Size);

        for (var i = 0; i < sampleCount; i++)
        {
            writer.Write(audioData.RawData[i]);
        }

        return memoryStream.ToArray();
    }
}