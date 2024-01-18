using Willow.Core.Eventing.Abstractions;
using Willow.Speech.Microphone.Eventing.Events;
using Willow.Speech.Resampling.Abstractions;
using Willow.Speech.VAD.Eventing.Interceptors;

namespace Willow.Speech.Resampling.Eventing.Interceptors;

/// <summary>
/// Intercepts the <see cref="AudioCapturedEvent" /> to 16Khz.
/// </summary>
internal sealed class ResamplingInterceptor : IEventInterceptor<AudioCapturedEvent>
{
    private const int DesiredSampleRate = 16000;
    private readonly ILogger<VoiceActivityDetectionInterceptor> _log;
    private readonly IResampler _resampler;

    public ResamplingInterceptor(IResampler resampler, ILogger<VoiceActivityDetectionInterceptor> log)
    {
        _resampler = resampler;
        _log = log;
    }

    public async Task InterceptAsync(AudioCapturedEvent @event, Func<AudioCapturedEvent, Task> next)
    {
        if (@event.AudioData.SamplingRate == DesiredSampleRate)
        {
            _log.ResamplingNotNeeded();
            await next(@event);
            return;
        }

        _log.Resampling(@event.AudioData.SamplingRate, DesiredSampleRate);
        var resampled = _resampler.Resample(@event.AudioData, DesiredSampleRate);
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
