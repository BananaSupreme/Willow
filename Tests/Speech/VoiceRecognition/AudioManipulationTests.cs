using Willow.Speech.SpeechRecognition.Microphone.Models;

namespace Tests.Speech.VoiceRecognition;

public class AudioManipulationTests
{
    [Fact]
    public void When_NormalizingData_ReturnsNormal()
    {
        var rawData = new short[10000];
        var random = new Random(1);
        rawData = rawData.Select(_ => (short)random.Next(0, short.MaxValue)).ToArray();
        var audioData = new AudioData(rawData, 1, 1, 1);
        audioData.NormalizedData.Should().BeEquivalentTo(rawData.Select(x => x / (float)32768.0));
    }
}