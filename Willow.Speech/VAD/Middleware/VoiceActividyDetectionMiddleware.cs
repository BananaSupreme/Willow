using Willow.Core.Middleware.Abstractions;
using Willow.Speech.AudioBuffering.Abstractions;
using Willow.Speech.Microphone.Eventing.Events;
using Willow.Speech.Microphone.Models;
using Willow.Speech.VAD.Abstractions;
using Willow.Speech.VAD.Models;

namespace Willow.Speech.VAD.Middleware;

/// <summary>
/// Detects speech in the audio, if found it buffers the audio until the user stops speaking or the buffer is full.
/// </summary>
internal sealed class VoiceActivityDetectionMiddleware : IMiddleware<AudioCapturedEvent>
{
    private readonly IAudioBuffer _audioBuffer;
    private readonly ILogger<VoiceActivityDetectionMiddleware> _log;
    private readonly IVoiceActivityDetection _vad;
    private AudioData _emptyAudioData;
    private AudioData _lastData;

    public VoiceActivityDetectionMiddleware(IVoiceActivityDetection vad,
                                            IAudioBuffer audioBuffer,
                                            ILogger<VoiceActivityDetectionMiddleware> log)
    {
        _vad = vad;
        _audioBuffer = audioBuffer;
        _log = log;
    }

    public async Task ExecuteAsync(AudioCapturedEvent input, Func<AudioCapturedEvent, Task> next)
    {
        _emptyAudioData = _emptyAudioData == default ? input.AudioData with { RawData = [] } : _emptyAudioData;
        var audioData = input.AudioData;
        var result = _vad.Detect(audioData);
        if (result.IsSpeechDetected && _audioBuffer.HasSpace(audioData.RawData.Length))
        {
            if (_audioBuffer.IsEmpty)
            {
                LoadDataLogged(_lastData, VoiceActivityResult.Failed());
                _lastData = _emptyAudioData;
            }

            LoadDataLogged(audioData, result);
            return;
        }

        if (_audioBuffer.IsEmpty)
        {
            _log.NoSpeechDetected();
            _lastData = audioData;
            return;
        }

        _lastData = _emptyAudioData;

        //If that's the case we didn't detect any sound but we did before, so we should add this last bit because
        //most of the time it contains the last bits of speech.
        if (_audioBuffer.HasSpace(audioData.RawData.Length))
        {
            LoadDataLogged(audioData, result);
        }

        var bufferedData = _audioBuffer.UnloadAllData();
        _log.UnloadedDataFromBuffer(bufferedData);

        //If this is true, so we ended here because the buffer ran out of space, we want to make sure we're not
        //losing samples here
        if (result.IsSpeechDetected)
        {
            LoadDataLogged(audioData, result);
        }

        await next(new AudioCapturedEvent(bufferedData));
    }

    private void LoadDataLogged(AudioData audioData, VoiceActivityResult result)
    {
        var startSample = audioData.FromTimeSpan(result.SpeechStart);
        var endSample = audioData.FromTimeSpan(result.SpeechEnd);
        _log.SpeechDetected(startSample, endSample);

        _log.LoadingDataIntoBuffer(audioData);
        //Load all the data so there won't be any weird cuts in the audio
        if (_audioBuffer.TryLoadData(audioData))
        {
            _log.DataLoadedIntoBuffer();
        }
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
}
