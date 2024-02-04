using Willow.Settings;
using Willow.Speech.AudioBuffering.Abstractions;
using Willow.Speech.AudioBuffering.Exceptions;
using Willow.Speech.AudioBuffering.Settings;
using Willow.Speech.Microphone.Models;

namespace Willow.Speech.AudioBuffering;

internal sealed class AudioBuffer : IAudioBuffer
{
    private readonly short[] _buffer;
    private ushort _bitDepthInBuffer;
    private ushort _channelCountInBuffer;
    private int _head;

    private int _samplingRateInBuffer;
    private int _size;
    private int _tail;

    public AudioBuffer(ISettings<AudioBufferSettings> settings)
    {
        _buffer = new short[settings.CurrentValue.MaxSeconds * settings.CurrentValue.AcceptedSamplingRate];
        _samplingRateInBuffer = settings.CurrentValue.AcceptedSamplingRate;
    }

    private int SpaceLeft => _buffer.Length - _size;
    public bool IsEmpty => _size == 0;

    public bool TryLoadData(AudioData audioData)
    {
        if (!IsEmpty && !IsSameAudioFeatures(audioData))
        {
            throw new MismatchedFeaturesException(audioData,
                                                  _samplingRateInBuffer,
                                                  _channelCountInBuffer,
                                                  _bitDepthInBuffer);
        }

        if (IsEmpty)
        {
            (_, _samplingRateInBuffer, _channelCountInBuffer, _bitDepthInBuffer) = audioData;
        }

        var sampleCount = audioData.RawData.Length;

        if (sampleCount > SpaceLeft)
        {
            return false;
        }

        var spaceUntiEndOfBuffer = _buffer.Length - _tail;

        if (spaceUntiEndOfBuffer >= sampleCount)
        {
            Array.Copy(audioData.RawData, 0, _buffer, _tail, audioData.RawData.Length);
        }
        else
        {
            Array.Copy(audioData.RawData, 0, _buffer, _tail, spaceUntiEndOfBuffer);
            Array.Copy(audioData.RawData,
                       spaceUntiEndOfBuffer,
                       _buffer,
                       0,
                       audioData.RawData.Length - spaceUntiEndOfBuffer);
        }

        _tail = (_tail + sampleCount) % _buffer.Length;
        _size += sampleCount;
        return true;
    }

    public (AudioData AudioData, int ActualSize) UnloadData(int maximumRequested)
    {
        if (_size == 0)
        {
            return (CreateAudioData([]), 0);
        }

        var actualSize = Math.Min(_size, maximumRequested);

        var arr = new short[actualSize];
        var spaceOccupiedUntilEndOfBuffer = _buffer.Length - _head;
        if (_head < _tail || actualSize < spaceOccupiedUntilEndOfBuffer)
        {
            Array.Copy(_buffer, _head, arr, 0, actualSize);
        }
        else
        {
            Array.Copy(_buffer, _head, arr, 0, spaceOccupiedUntilEndOfBuffer);
            Array.Copy(_buffer, 0, arr, spaceOccupiedUntilEndOfBuffer, actualSize - spaceOccupiedUntilEndOfBuffer);
        }

        _size -= actualSize;
        _head = (_head + actualSize) % _buffer.Length;
        return (CreateAudioData(arr), actualSize);
    }

    public AudioData UnloadAllData()
    {
        if (_size == 0)
        {
            return CreateAudioData([]);
        }

        var arr = new short[_size];
        if (_head < _tail)
        {
            Array.Copy(_buffer, _head, arr, 0, _size);
        }
        else
        {
            var spaceOccupiedUntilEndOfBuffer = _buffer.Length - _head;
            Array.Copy(_buffer, _head, arr, 0, spaceOccupiedUntilEndOfBuffer);
            Array.Copy(_buffer, 0, arr, spaceOccupiedUntilEndOfBuffer, _tail);
        }

        _tail = _head = _size = 0;
        return CreateAudioData(arr);
    }

    public bool HasSpace(int space)
    {
        return space <= _buffer.Length - _size;
    }

    private AudioData CreateAudioData(short[] rawData)
    {
        return new AudioData(rawData, _samplingRateInBuffer, _channelCountInBuffer, _bitDepthInBuffer);
    }

    private bool IsSameAudioFeatures(AudioData audioData)
    {
        return audioData.ChannelCount == _channelCountInBuffer
               && audioData.SamplingRate == _samplingRateInBuffer
               && audioData.BitDepth == _bitDepthInBuffer;
    }
}
