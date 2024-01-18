using Willow.Speech.Microphone.Models;

namespace Willow.Speech.Microphone.Extensions;

internal static class RawAudioDataExtension
{
    private const int Subchunk1Size = 16; //Standard
    private const ushort PcmAudioFormat = 1; //Standard
    private const int HeaderSize = 36; //Standard
    private static readonly byte[] _riffHeader = "RIFF"u8.ToArray();
    private static readonly byte[] _waveHeader = "WAVE"u8.ToArray();
    private static readonly byte[] _fmtHeader = "fmt "u8.ToArray();
    private static readonly byte[] _dataHeader = "data"u8.ToArray();

    /// <summary>
    /// Converts <see cref="AudioData" /> to a proper WAV file.
    /// </summary>
    /// <param name="audioData">The input data.</param>
    /// <returns>Array of bytes representing a WAV file.</returns>
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
        writer.Write(HeaderSize + subchunk2Size);
        writer.Write(_waveHeader);

        // fmt Sub-chunk
        writer.Write(_fmtHeader);
        writer.Write(Subchunk1Size);
        writer.Write(PcmAudioFormat);
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
