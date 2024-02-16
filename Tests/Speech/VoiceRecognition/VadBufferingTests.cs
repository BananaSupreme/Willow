using Tests.Helpers;

using Willow.Settings;
using Willow.Speech.Microphone.Events;
using Willow.Speech.Microphone.Models;
using Willow.Speech.VAD.Abstractions;
using Willow.Speech.VAD.Middleware;
using Willow.Speech.VAD.Models;
using Willow.Speech.VAD.Settings;

using Xunit.Abstractions;

namespace Tests.Speech.VoiceRecognition;

public sealed class VadBufferingTests : IDisposable
{
    private readonly ISettings<BufferingSettings> _settings;
    private readonly IVoiceActivityDetection _vad;
    private readonly IServiceProvider _provider;
    private readonly AudioData _failingData = new([0], 1, 1, 1);
    private readonly AudioData _successData = new([1], 1, 1, 1);
    private int _nextCaptureInvocationCount;

    public VadBufferingTests(ITestOutputHelper outputHelper)
    {
        _settings = Substitute.For<ISettings<BufferingSettings>>();
        _settings.CurrentValue.Returns(new BufferingSettings(0, 1, 5));
        _vad = Substitute.For<IVoiceActivityDetection>();
        _vad.Detect(_failingData).Returns(VoiceActivityResult.Failed());
        _vad.Detect(_successData).Returns(VoiceActivityResult.Success(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1)));
        var services = new ServiceCollection();
        services.AddSingleton(_vad);
        services.AddTestLogger(outputHelper);
        services.AddSingleton(_settings);
        services.AddSingleton<VoiceActivityDetectionMiddleware>();
        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task When_EmptyVadAcceptsFailingAudio_NotForwardedOrSaved()
    {
        var sut = _provider.GetRequiredService<VoiceActivityDetectionMiddleware>();

        await SendFailure(sut, NextFail);
        sut.Dump().RawData.Should().BeEmpty();
    }

    [Fact]
    public async Task When_DetectingSpeech_AudioNotForwardedButStored()
    {
        var sut = _provider.GetRequiredService<VoiceActivityDetectionMiddleware>();

        await SendSuccess(sut, NextFail);
        sut.Dump().RawData.Should().HaveCount(1);
    }

    [Fact]
    public async Task When_VadWithAudioAcceptsEmptyAudioAndNoFailureAllowed_ForwardAndIncludeFailedAudio()
    {
        var sut = _provider.GetRequiredService<VoiceActivityDetectionMiddleware>();

        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendFailure(sut, NextCaptureCount(3));
        _nextCaptureInvocationCount.Should().Be(1);
        sut.Dump().RawData.Should().BeEmpty();
    }

    [Fact]
    public async Task When_VadFailsForAmountAllowedUnderSettings_NotConsideredUnloaded()
    {
        _settings.CurrentValue.Returns(new BufferingSettings(1, 1, 5));
        var sut = _provider.GetRequiredService<VoiceActivityDetectionMiddleware>();

        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendFailure(sut, NextFail);
        await SendSuccess(sut, NextFail);
    }

    [Fact]
    public async Task When_VadFailureSkip_CountsTowardsFull()
    {
        _settings.CurrentValue.Returns(new BufferingSettings(1, 1, 5));
        var sut = _provider.GetRequiredService<VoiceActivityDetectionMiddleware>();

        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendFailure(sut, NextFail);
        await SendSuccess(sut, NextFail);
        sut.Dump().RawData.Should().HaveCount(4);
    }

    [Fact]
    public async Task When_VadIsFull_UnloadsAudio()
    {
        var sut = _provider.GetRequiredService<VoiceActivityDetectionMiddleware>();

        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextCaptureCount(5));
        _nextCaptureInvocationCount.Should().Be(1);
    }

    [Fact]
    public async Task When_VadFilled_AudioNotSkipped()
    {
        var sut = _provider.GetRequiredService<VoiceActivityDetectionMiddleware>();

        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextCaptureCount(5));
        sut.Dump().RawData.Should().HaveCount(1);
        _nextCaptureInvocationCount.Should().Be(1);
    }

    [Fact]
    public async Task When_LoadingAudioPreviousFailedAudio_IsCaptured()
    {
        var sut = _provider.GetRequiredService<VoiceActivityDetectionMiddleware>();

        await SendFailure(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendFailure(sut, NextCaptureCount(4));
        _nextCaptureInvocationCount.Should().Be(1);
    }

    [Fact]
    public async Task When_VadUnloads_FailureLimitResets()
    {
        _settings.CurrentValue.Returns(new BufferingSettings(2, 1, 10));
        var sut = _provider.GetRequiredService<VoiceActivityDetectionMiddleware>();

        await SendSuccess(sut, NextFail);

        await SendFailure(sut, NextFail);
        await SendFailure(sut, NextFail);

        await SendSuccess(sut, NextFail);

        await SendFailure(sut, NextFail);
        await SendFailure(sut, NextFail);
        await SendFailure(sut, NextCaptureCount(7));

        await SendSuccess(sut, NextFail);

        await SendFailure(sut, NextFail);

        await SendSuccess(sut, NextFail);

        await SendFailure(sut, NextFail);
        await SendFailure(sut, NextFail);

        await SendSuccess(sut, NextFail);

        await SendFailure(sut, NextFail);
        await SendFailure(sut, NextFail);
        await SendFailure(sut, NextCaptureCount(9));

        _nextCaptureInvocationCount.Should().Be(2);
    }

    [Fact]
    public async Task When_VadAcceptsAudioWithDifferentFeatures_SamplingRate_EmptyAudioAndStartFillingWithNewFeatures()
    {
        var differentFeatures = new AudioData([0], 2, 1, 1);
        _vad.Detect(differentFeatures).Returns(VoiceActivityResult.Failed());
        var sut = _provider.GetRequiredService<VoiceActivityDetectionMiddleware>();

        await SendFailure(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await sut.ExecuteAsync(new AudioCapturedEvent(differentFeatures), NextCaptureCount(3));
        _nextCaptureInvocationCount.Should().Be(1);
        var dump = sut.Dump();
        dump.RawData.Should().BeEmpty();
        dump.SamplingRate.Should().Be(2);
    }

    [Fact]
    public async Task When_VadAcceptsAudioWithDifferentFeatures_ChannelCount_EmptyAudioAndStartFillingWithNewFeatures()
    {
        var differentFeatures = new AudioData([0], 1, 2, 1);
        _vad.Detect(differentFeatures).Returns(VoiceActivityResult.Failed());
        var sut = _provider.GetRequiredService<VoiceActivityDetectionMiddleware>();

        await SendFailure(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await sut.ExecuteAsync(new AudioCapturedEvent(differentFeatures), NextCaptureCount(3));
        _nextCaptureInvocationCount.Should().Be(1);
        var dump = sut.Dump();
        dump.RawData.Should().BeEmpty();
        dump.ChannelCount.Should().Be(2);
    }

    [Fact]
    public async Task When_VadAcceptsAudioWithDifferentFeatures_BitDepth_EmptyAudioAndStartFillingWithNewFeatures()
    {
        var differentFeatures = new AudioData([0], 1, 1, 2);
        _vad.Detect(differentFeatures).Returns(VoiceActivityResult.Failed());
        var sut = _provider.GetRequiredService<VoiceActivityDetectionMiddleware>();

        await SendFailure(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await sut.ExecuteAsync(new AudioCapturedEvent(differentFeatures), NextCaptureCount(3));
        _nextCaptureInvocationCount.Should().Be(1);
        var dump = sut.Dump();
        dump.RawData.Should().BeEmpty();
        dump.BitDepth.Should().Be(2);
    }

    [Fact]
    public async Task When_unloading_FeaturesRemainTheSame()
    {
        var sut = _provider.GetRequiredService<VoiceActivityDetectionMiddleware>();

        await SendFailure(sut, NextFail);
        await SendSuccess(sut, NextFail);
        await SendFailure(sut, NextTestCapture);

        _nextCaptureInvocationCount.Should().Be(1);
        return;

        Task NextTestCapture(AudioCapturedEvent data)
        {
            _nextCaptureInvocationCount++;
            data.AudioData.Should().BeEquivalentTo(_successData with { RawData = [0, 1, 0] });
            return Task.CompletedTask;
        }
    }

    private async Task SendFailure(VoiceActivityDetectionMiddleware sut, Func<AudioCapturedEvent, Task> next)
    {
        await sut.ExecuteAsync(new AudioCapturedEvent(_failingData), next);
    }

    private async Task SendSuccess(VoiceActivityDetectionMiddleware sut, Func<AudioCapturedEvent, Task> next)
    {
        await sut.ExecuteAsync(new AudioCapturedEvent(_successData), next);
    }

    private static Task NextFail(AudioCapturedEvent _)
    {
        Assert.Fail();
        return Task.CompletedTask;
    }

    private Func<AudioCapturedEvent, Task> NextCaptureCount(int capturedCount)
    {
        return data =>
        {
            _nextCaptureInvocationCount++;
            data.AudioData.RawData.Should().HaveCount(capturedCount);
            return Task.CompletedTask;
        };
    }

    //When_unloading_FeaturesRemainTheSame
    public void Dispose()
    {
        (_provider as IDisposable)?.Dispose();
    }
}
