using Willow.Core.Eventing.Abstractions;
using Willow.Speech.Microphone.Eventing.Events;
using Willow.Speech.Resampling.Abstractions;
using Willow.Speech.VAD.Eventing.Interceptors;

namespace Willow.Speech.Resampling.Eventing.Interceptors;

internal sealed class ResamplingInterceptor : IEventInterceptor<AudioCapturedEvent>
{
    private const int _desiredSampleRate = 16000;
    private readonly IResampler _resampler;
    private readonly ILogger<VoiceActivityDetectionInterceptor> _log;

    public ResamplingInterceptor(IResampler resampler, ILogger<VoiceActivityDetectionInterceptor> log)
    {
        _resampler = resampler;
        _log = log;
    }

    public async Task InterceptAsync(AudioCapturedEvent @event, Func<AudioCapturedEvent, Task> next)
    {
        if (@event.AudioData.SamplingRate == _desiredSampleRate)
        {
            _log.ResamplingNotNeeded();
            await next(@event);
            return;
        }

        _log.Resampling(@event.AudioData.SamplingRate, _desiredSampleRate);
        var resampled = _resampler.Resample(@event.AudioData, _desiredSampleRate);
        await next(new(resampled));
    }
}

internal static partial class VoicesActivityDetectionLoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Audio at correct sample rate..")]
    public static partial void ResamplingNotNeeded(this ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "Audio at wrong sampling rate ({inputSampleRate}), resampling ({outputSampleRate}).")]
    public static partial void Resampling(this ILogger logger, int inputSampleRate, int outputSampleRate);
}