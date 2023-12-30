using Microsoft.Extensions.Options;

using Willow.Speech.SpeechRecognition.AudioBuffering;
using Willow.Speech.SpeechRecognition.AudioBuffering.Settings;
using Willow.Speech.SpeechRecognition.Microphone.Models;

namespace Tests.VoiceCommands.VoiceRecognition;

public class AudioBufferTests
{
    private readonly AudioData _baseData;
    private readonly AudioBuffer _sut;

    public AudioBufferTests()
    {
        var settings = Substitute.For<IOptionsMonitor<AudioBufferSettings>>();
        settings.CurrentValue.Returns(new AudioBufferSettings { AcceptedSamplingRate = 10, MaxSeconds = 1 });
        _baseData = new([], settings.CurrentValue.AcceptedSamplingRate, 1, 16);
        _sut = new(settings);
    }

    [Fact]
    public void When_BufferHasEnoughSpace_EnqueueData_ReturnsTrue()
    {
        var data = _baseData with { RawData = new short[5] };
        _sut.TryLoadData(data).Should().BeTrue();
    }

    [Fact]
    public void When_BufferDoesNotHaveEnoughSpace_EnqueueData_ReturnsFalse()
    {
        var data = _baseData with { RawData = new short[7] };
        _sut.TryLoadData(data).Should().BeTrue();
        _sut.TryLoadData(data).Should().BeFalse();
    }

    [Fact]
    public void When_LoadingDifferentAudioFeaturesFromSettings_ThrowsException()
    {
        var data = _baseData with { RawData = new short[1] };
        _sut.TryLoadData(data).Should().BeTrue();
        data = _baseData with { RawData = new short[1], SamplingRate = 100 };
        _sut.Invoking(x => x.TryLoadData(data)).Should().Throw<Exception>();
    }

    [Fact]
    public void When_LoadingDifferentAudioFeaturesFromPrevious_ThrowsException()
    {
        var data = _baseData with { RawData = new short[1] };
        _sut.TryLoadData(data).Should().BeTrue();
        data = _baseData with { RawData = new short[1], BitDepth = 100 };
        _sut.Invoking(x => x.TryLoadData(data)).Should().Throw<Exception>();
    }

    [Fact]
    public void When_Loading_SizeIsReducedByLoad()
    {
        var data = _baseData with { RawData = new short[7] };
        _sut.TryLoadData(data).Should().BeTrue();
        TestSize(7);
    }

    [Fact]
    public void When_UnloadingLoadedData_LoadedDataIsReturned()
    {
        var data = _baseData with { RawData = [1, 2] };
        _sut.TryLoadData(data).Should().BeTrue();
        var unloadedData = _sut.UnloadAllData();
        data.Should().BeEquivalentTo(unloadedData);
    }

    [Fact]
    public void When_LoadWrapsAround_SizeIsCorrect()
    {
        var data = _baseData with { RawData = new short[7] };
        _sut.TryLoadData(data).Should().BeTrue();
        _ = _sut.UnloadData(5);
        _sut.TryLoadData(data).Should().BeTrue();
        TestSize(9);
    }

    [Fact]
    public void When_UnloadWrapsAround_SizeIsCorrect()
    {
        var data = _baseData with { RawData = new short[7] };
        _sut.TryLoadData(data).Should().BeTrue();
        _ = _sut.UnloadData(5);
        _sut.TryLoadData(data).Should().BeTrue();
        _ = _sut.UnloadData(7);
        TestSize(2);
    }

    [Fact]
    public void When_LoadingTwice_SizeIsCorrect()
    {
        var data = _baseData with { RawData = new short[3] };
        _sut.TryLoadData(data).Should().BeTrue();
        _sut.TryLoadData(data).Should().BeTrue();
        TestSize(6);
    }

    [Fact]
    public void When_RequestingMoreThanSize_ReturnsWhatIsAvailable()
    {
        var data = _baseData with { RawData = new short[3] };
        _sut.TryLoadData(data).Should().BeTrue();
        var result = _sut.UnloadData(7);
        result.ActualSize.Should().Be(3);
    }

    [Fact]
    public void When_RequestingLessThanSize_ReturnsRequestedAmount()
    {
        var data = _baseData with { RawData = new short[9] };
        _sut.TryLoadData(data).Should().BeTrue();
        var result = _sut.UnloadData(7);
        result.ActualSize.Should().Be(7);
    }

    [Fact]
    public void When_UnloadingAllData_SizeIsZero()
    {
        var data = _baseData with { RawData = new short[3] };
        _sut.TryLoadData(data).Should().BeTrue();
        var result = _sut.UnloadAllData();
        result.RawData.Length.Should().Be(3);
        TestSize(0);
    }

    [Fact]
    public void When_HeadAndTailWrapped_UnloadAllDataReturnsCorrectAmountOfData()
    {
        var data = _baseData with { RawData = new short[7] };
        _sut.TryLoadData(data).Should().BeTrue();
        _ = _sut.UnloadData(5);
        _sut.TryLoadData(data).Should().BeTrue();
        _ = _sut.UnloadData(7);
        var result = _sut.UnloadAllData();
        result.RawData.Length.Should().Be(2);
    }

    [Fact]
    public void When_UnloadingEmptyQueue_NoExceptions()
    {
        var data = _baseData with { RawData = new short[7] };
        _sut.TryLoadData(data).Should().BeTrue();
        _ = _sut.UnloadData(7);
        _ = _sut.UnloadData(7);
        _ = _sut.UnloadAllData();
    }

    private void TestSize(int size)
    {
        _sut.HasSpace(10 - size).Should().BeTrue();
        _sut.HasSpace(10 - size + 1).Should().BeFalse();
    }
}