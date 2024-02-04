using Willow.Middleware;
using Willow.Speech.Microphone.Events;
using Willow.Speech.Resampling.Abstractions;
using Willow.Speech.VAD.Middleware;

namespace Willow.Speech.Resampling.Middleware;

/// <summary>
/// Intercepts the <see cref="AudioCapturedEvent" /> to 16Khz.
/// </summary>
internal sealed class ResamplingMiddleware : IMiddleware<AudioCapturedEvent>
{
    private const int DesiredSampleRate = 16000;
    private readonly ILogger<VoiceActivityDetectionMiddleware> _log;
    private readonly IResampler _resampler;

    public ResamplingMiddleware(IResampler resampler, ILogger<VoiceActivityDetectionMiddleware> log)
    {
        _resampler = resampler;
        _log = log;
    }

    public async Task ExecuteAsync(AudioCapturedEvent input, Func<AudioCapturedEvent, Task> next)
    {
        if (input.AudioData.SamplingRate == DesiredSampleRate)
        {
            _log.ResamplingNotNeeded();
            await next(input);
            return;
        }

        _log.Resampling(input.AudioData.SamplingRate, DesiredSampleRate);
        var resampled = _resampler.Resample(input.AudioData, DesiredSampleRate);
        await next(new AudioCapturedEvent(resampled));
    }
}

internal static partial class VoicesActivityDetectionLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Audio at correct sample rate..")]
    public static partial void ResamplingNotNeeded(this ILogger logger);

    [LoggerMessage(EventId = 2,
                   Level = LogLevel.Debug,
                   Message = "Audio at wrong sampling rate ({inputSampleRate}), resampling ({outputSampleRate}).")]
    public static partial void Resampling(this ILogger logger, int inputSampleRate, int outputSampleRate);
}
