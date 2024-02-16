using Willow.Helpers.DataStructures;

namespace Tests.Speech.VoiceRecognition;

public sealed class CircularBufferTests
{
    private readonly CircularBuffer<short> _sut = new(10);

    [Fact]
    public void When_BufferHasEnoughSpace_EnqueueData_ReturnsTrue()
    {
        _sut.TryLoadData(new short[5]).Should().BeTrue();
    }

    [Fact]
    public void When_BufferDoesNotHaveEnoughSpace_EnqueueData_ReturnsFalse()
    {
        _sut.TryLoadData(new short[7]).Should().BeTrue();
        _sut.TryLoadData(new short[7]).Should().BeFalse();
    }

    [Fact]
    public void When_Loading_SizeIsReducedByLoad()
    {
        _sut.TryLoadData(new short[7]).Should().BeTrue();
        TestSize(7);
    }

    [Fact]
    public void When_UnloadingLoadedData_LoadedDataIsReturned()
    {
        short[] data = [1, 2];
        _sut.TryLoadData(data).Should().BeTrue();
        var unloadedData = _sut.UnloadAllData();
        data.Should().BeEquivalentTo(unloadedData);
    }

    [Fact]
    public void When_LoadWrapsAround_SizeIsCorrect()
    {
        _sut.TryLoadData(new short[7]).Should().BeTrue();
        _ = _sut.UnloadData(5);
        _sut.TryLoadData(new short[7]).Should().BeTrue();
        TestSize(9);
    }

    [Fact]
    public void When_UnloadWrapsAround_SizeIsCorrect()
    {
        _sut.TryLoadData(new short[7]).Should().BeTrue();
        _ = _sut.UnloadData(5);
        _sut.TryLoadData(new short[7]).Should().BeTrue();
        _ = _sut.UnloadData(7);
        TestSize(2);
    }

    [Fact]
    public void When_LoadingTwice_SizeIsCorrect()
    {
        _sut.TryLoadData(new short[3]).Should().BeTrue();
        _sut.TryLoadData(new short[3]).Should().BeTrue();
        TestSize(6);
    }

    [Fact]
    public void When_RequestingMoreThanSize_ReturnsWhatIsAvailable()
    {
        _sut.TryLoadData(new short[3]).Should().BeTrue();
        var result = _sut.UnloadData(7);
        result.ActualSize.Should().Be(3);
    }

    [Fact]
    public void When_RequestingLessThanSize_ReturnsRequestedAmount()
    {
        _sut.TryLoadData(new short[9]).Should().BeTrue();
        var result = _sut.UnloadData(7);
        result.ActualSize.Should().Be(7);
    }

    [Fact]
    public void When_UnloadingAllData_SizeIsZero()
    {
        _sut.TryLoadData(new short[3]).Should().BeTrue();
        var result = _sut.UnloadAllData();
        result.Length.Should().Be(3);
        TestSize(0);
    }

    [Fact]
    public void When_HeadAndTailWrapped_UnloadAllDataReturnsCorrectAmountOfData()
    {
        _sut.TryLoadData(new short[7]).Should().BeTrue();
        _ = _sut.UnloadData(5);
        _sut.TryLoadData(new short[7]).Should().BeTrue();
        _ = _sut.UnloadData(7);
        var result = _sut.UnloadAllData();
        result.Length.Should().Be(2);
    }

    [Fact]
    public void When_UnloadingEmptyQueue_NoExceptions()
    {
        _sut.TryLoadData(new short[7]).Should().BeTrue();
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
