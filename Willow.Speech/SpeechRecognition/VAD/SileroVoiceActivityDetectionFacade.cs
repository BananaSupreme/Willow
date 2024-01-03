using SileroVad;

using Willow.Core.Settings.Abstractions;
using Willow.Speech.SpeechRecognition.Microphone.Models;
using Willow.Speech.SpeechRecognition.VAD.Abstractions;
using Willow.Speech.SpeechRecognition.VAD.Models;
using Willow.Speech.SpeechRecognition.VAD.Settings;

namespace Willow.Speech.SpeechRecognition.VAD;

internal sealed class SileroVoiceActivityDetectionFacade : IVoiceActivityDetection, IDisposable
{
    private readonly ILogger<SileroVoiceActivityDetectionFacade> _log;
    private readonly ISettings<SileroSettings> _sileroSettings;
    private readonly Vad _sileroVad = new();

    public SileroVoiceActivityDetectionFacade(ISettings<SileroSettings> sileroSettings,
                                              ILogger<SileroVoiceActivityDetectionFacade> log)
    {
        _sileroSettings = sileroSettings;
        _log = log;
    }

    public void Dispose()
    {
        _sileroVad.Dispose();
    }

    public VoiceActivityResult Detect(AudioData audioSegment)
    {
        var currentValue = _sileroSettings.CurrentValue;
        _log.VoiceActivityDetectionStarted(currentValue);
        var result = _sileroVad.GetSpeechTimestamps(audioSegment.NormalizedData,
            currentValue.Threshold,
            currentValue.MinSpeechDurationMilliseconds,
            currentValue.MaxSpeechDurationSeconds,
            currentValue.MinSilenceDurationMilliseconds,
            currentValue.WindowSizeSamples,
            currentValue.SpeechPadMilliseconds);
        
        var vadResult = ProcessVadResult(audioSegment, result);

        _log.VoiceActivityDetectionResult(vadResult);
        return vadResult;
    }

    private static VoiceActivityResult ProcessVadResult(AudioData audioSegment, List<VadSpeech> result)
    {
        VoiceActivityResult vadResult;
        if (result.Count > 0)
        {
            var speechStart = audioSegment.FromSamplePosition(result[0].Start);
            var speechEnd = audioSegment.FromSamplePosition(result[^1].End);
            vadResult = VoiceActivityResult.Success(speechStart, speechEnd);
        }
        else
        {
            vadResult = VoiceActivityResult.Failed();
        }

        return vadResult;
    }
}

internal static partial class SileroVadLoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Starting voice activity detection using Silero model with settings: {settings}.")]
    public static partial void VoiceActivityDetectionStarted(this ILogger logger, SileroSettings settings);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "Ended detection with result: {result}.")]
    public static partial void VoiceActivityDetectionResult(this ILogger logger, VoiceActivityResult result);
}