using Willow.Helpers.DataStructures;
using Willow.Middleware;
using Willow.Settings;
using Willow.Speech.Microphone.Events;
using Willow.Speech.Microphone.Models;
using Willow.Speech.VAD.Abstractions;
using Willow.Speech.VAD.Models;
using Willow.Speech.VAD.Settings;

namespace Willow.Speech.VAD.Middleware;

/// <summary>
/// Detects speech in the audio, if found it buffers the audio until the user stops speaking or the buffer is full.
/// </summary>
internal sealed class VoiceActivityDetectionMiddleware : IMiddleware<AudioCapturedEvent>
{
    private readonly CircularBuffer<short> _audioBuffer;
    private readonly ISettings<BufferingSettings> _settings;
    private readonly ILogger<VoiceActivityDetectionMiddleware> _log;
    private readonly IVoiceActivityDetection _vad;
    private AudioData _emptyAudioData = AudioData.Empty;
    private AudioData _lastData = AudioData.Empty;
    private int _failedAudioTranscriptions;

    public VoiceActivityDetectionMiddleware(IVoiceActivityDetection vad,
                                            ISettings<BufferingSettings> settings,
                                            ILogger<VoiceActivityDetectionMiddleware> log)
    {
        _vad = vad;
        _settings = settings;
        _log = log;
        _audioBuffer = new CircularBuffer<short>(settings.CurrentValue.AcceptedSamplingRate
                                                 * settings.CurrentValue.MaxSeconds);
    }

    public async Task ExecuteAsync(AudioCapturedEvent input, Func<AudioCapturedEvent, Task> next)
    {
        if (!IsSameAudioFeatures(input.AudioData))
        {
            await EnsureSameFeatures(input, next);
        }

        var audioData = input.AudioData;
        var result = _vad.Detect(audioData);

        if (TryBufferingData(result, audioData))
        {
            return;
        }

        var bufferedData = _audioBuffer.UnloadAllData();
        _log.UnloadedDataFromBuffer(CreateAudioData(bufferedData));

        //If this is true, so we ended here because the buffer ran out of space (we're testing for both in
        //TryBufferingData), we want to make sure we're not losing samples here
        if (result.IsSpeechDetected)
        {
            LoadDataLogged(audioData, result);
        }

        _failedAudioTranscriptions = 0;
        await next(new AudioCapturedEvent(CreateAudioData(bufferedData)));
    }

    //The order really matters here, since we are checking things that were checked before in later cases, but not in the
    //same combination
    private bool TryBufferingData(VoiceActivityResult result, AudioData audioData)
    {
        if (TryCaseDetectionSucceededAndBufferHasSpace(result, audioData))
        {
            return true;
        }

        if (TryCaseBufferIsEmpty(audioData))
        {
            return true;
        }

        _lastData = _emptyAudioData;

        if (TryCaseBufferHasSpaceAndWithinLimitOfFaultTolerance(result, audioData))
        {
            return true;
        }

        return false;
    }

    private bool TryCaseBufferHasSpaceAndWithinLimitOfFaultTolerance(VoiceActivityResult result, AudioData audioData)
    {
        //If that's the case we didn't detect any sound but we did before, so we should add this last bit because
        //most of the time it contains the last bits of speech.
        if (_audioBuffer.HasSpace(audioData.RawData.Length))
        {
            LoadDataLogged(audioData, result);
            if (_settings.CurrentValue.VadFalseTolerance > _failedAudioTranscriptions)
            {
                _failedAudioTranscriptions++;
                return true;
            }
        }

        return false;
    }

    private bool TryCaseBufferIsEmpty(AudioData audioData)
    {
        if (_audioBuffer.IsEmpty)
        {
            _log.NoSpeechDetected();
            _lastData = audioData;
            return true;
        }

        return false;
    }

    private bool TryCaseDetectionSucceededAndBufferHasSpace(VoiceActivityResult result, AudioData audioData)
    {
        if (result.IsSpeechDetected && _audioBuffer.HasSpace(audioData.RawData.Length))
        {
            if (_audioBuffer.IsEmpty && _lastData != default)
            {
                LoadDataLogged(_lastData, VoiceActivityResult.Failed());
                _lastData = _emptyAudioData;
            }

            _failedAudioTranscriptions = 0;
            LoadDataLogged(audioData, result);
            return true;
        }

        return false;
    }

    private async Task EnsureSameFeatures(AudioCapturedEvent input, Func<AudioCapturedEvent, Task> next)
    {
        _log.FeaturesChanged(_emptyAudioData, input.AudioData);
        _emptyAudioData = input.AudioData with { RawData = [] };
        if (!_audioBuffer.IsEmpty)
        {
            var buffer = _audioBuffer.UnloadAllData();
            var dataInBuffer = CreateAudioData(buffer);
            _log.FeaturesChangedBufferUnloaded(dataInBuffer);
            await next(new AudioCapturedEvent(dataInBuffer));
        }
    }

    /// <summary>
    /// Used for testing.
    /// </summary>
    /// <returns>The contents of the audio buffer held inside.</returns>
    internal AudioData Dump()
    {
        return CreateAudioData(_audioBuffer.UnloadAllData());
    }

    private void LoadDataLogged(AudioData audioData, VoiceActivityResult result)
    {
        var startSample = audioData.FromTimeSpan(result.SpeechStart);
        var endSample = audioData.FromTimeSpan(result.SpeechEnd);
        _log.SpeechDetected(startSample, endSample);

        _log.LoadingDataIntoBuffer(audioData);
        //Load all the data so there won't be any weird cuts in the audio
        if (_audioBuffer.TryLoadData(audioData.RawData))
        {
            _log.DataLoadedIntoBuffer();
        }
    }

    private bool IsSameAudioFeatures(AudioData audioData)
    {
        return audioData.ChannelCount == _emptyAudioData.ChannelCount
               && audioData.SamplingRate == _emptyAudioData.SamplingRate
               && audioData.BitDepth == _emptyAudioData.BitDepth;
    }

    private AudioData CreateAudioData(short[] data)
    {
        return _emptyAudioData with { RawData = data };
    }
}

internal static partial class VoiceActivityDetectionMiddlewareLoggingExtensions
{
    [LoggerMessage(EventId = 1,
                   Level = LogLevel.Debug,
                   Message = "No speech detected in AudioCapturedEvent, skipping further processing.")]
    public static partial void NoSpeechDetected(this ILogger logger);

    [LoggerMessage(EventId = 2,
                   Level = LogLevel.Debug,
                   Message = "Speech detected in AudioCapturedEvent from {startSample} to {endSample}.")]
    public static partial void SpeechDetected(this ILogger logger, int startSample, int endSample);

    [LoggerMessage(EventId = 3,
                   Level = LogLevel.Debug,
                   Message = "Attempting to load audio data into buffer. AudioData: {audioData}.")]
    public static partial void LoadingDataIntoBuffer(this ILogger logger, AudioData audioData);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Audio data successfully loaded into buffer.")]
    public static partial void DataLoadedIntoBuffer(this ILogger logger);

    [LoggerMessage(EventId = 5,
                   Level = LogLevel.Information,
                   Message = "Unloaded data from buffer for further processing. AudioData: {audioData}.")]
    public static partial void UnloadedDataFromBuffer(this ILogger logger, AudioData audioData);

    [LoggerMessage(EventId = 6,
                   Level = LogLevel.Warning,
                   Message = "Features were changed, resetting state. from ({oldAudioData}) to ({newAudioData})")]
    public static partial void FeaturesChanged(this ILogger logger, AudioData oldAudioData, AudioData newAudioData);

    [LoggerMessage(EventId = 7,
                   Level = LogLevel.Warning,
                   Message
                       = "Features were changed, with data in the buffer, sending the data that was already in the buffer. ({dataInBuffer})")]
    public static partial void FeaturesChangedBufferUnloaded(this ILogger logger, AudioData dataInBuffer);
}
