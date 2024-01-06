using Willow.Core.Eventing.Abstractions;
using Willow.Speech.SpeechRecognition.AudioBuffering.Abstractions;
using Willow.Speech.SpeechRecognition.Microphone.Eventing.Events;
using Willow.Speech.SpeechRecognition.Microphone.Models;
using Willow.Speech.SpeechRecognition.VAD.Abstractions;

namespace Willow.Speech.SpeechRecognition.VAD.Eventing.Interceptors;

internal sealed class VoiceActivityDetectionInterceptor : IEventInterceptor<AudioCapturedEvent>
{
    private readonly IAudioBuffer _audioBuffer;
    private readonly ILogger<VoiceActivityDetectionInterceptor> _log;
    private readonly IVoiceActivityDetection _vad;

    public VoiceActivityDetectionInterceptor(IVoiceActivityDetection vad,
                                             IAudioBuffer audioBuffer,
                                             ILogger<VoiceActivityDetectionInterceptor> log)
    {
        _vad = vad;
        _audioBuffer = audioBuffer;
        _log = log;
    }

    public async Task InterceptAsync(AudioCapturedEvent @event, Func<AudioCapturedEvent, Task> next)
    {
        var audioData = @event.AudioData;
        var result = _vad.Detect(audioData);
        if (result.IsSpeechDetected
            && _audioBuffer.HasSpace(audioData.RawData.Length))
        {
            var startSample = audioData.FromTimeSpan(result.SpeechStart);
            var endSample = audioData.FromTimeSpan(result.SpeechEnd);
            _log.SpeechDetected(startSample, endSample);

            //Load all the data so there won't be any weird cuts in the audio
            LoadDataLogged(audioData);
        }
        else
        {
            var bufferedData = _audioBuffer.UnloadAllData();
            if (bufferedData.RawData.Length == 0)
            {
                _log.NoSpeechDetected();
                return;
            }

            _log.UnloadedDataFromBuffer(bufferedData);
            //If this is true, so we ended here because the buffer ran out of space, we want to make sure we're not losing samples here
            if (result.IsSpeechDetected)
            {
                LoadDataLogged(audioData);
            }

            await next(new(bufferedData));
        }
    }

    private void LoadDataLogged(AudioData speechData)
    {
        _log.LoadingDataIntoBuffer(speechData);
        if (_audioBuffer.TryLoadData(speechData))
        {
            _log.DataLoadedIntoBuffer();
        }
    }
}

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "No speech detected in AudioCapturedEvent, skipping further processing.")]
    public static partial void NoSpeechDetected(this ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "Speech detected in AudioCapturedEvent from {startSample} to {endSample}.")]
    public static partial void SpeechDetected(this ILogger logger, int startSample, int endSample);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Attempting to load audio data into buffer. AudioData: {audioData}.")]
    public static partial void LoadingDataIntoBuffer(this ILogger logger, AudioData audioData);


    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Audio data successfully loaded into buffer.")]
    public static partial void DataLoadedIntoBuffer(this ILogger logger);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Unloaded data from buffer for further processing. AudioData: {audioData}.")]
    public static partial void UnloadedDataFromBuffer(this ILogger logger, AudioData audioData);
}