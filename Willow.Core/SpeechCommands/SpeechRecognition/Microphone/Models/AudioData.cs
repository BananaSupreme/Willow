namespace Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Models;

public record struct AudioData(short[] RawData, int SamplingRate, ushort ChannelCount, ushort BitDepth)
{
    public TimeSpan Duration => FromSamplePosition(RawData.Length);
    public float[] NormalizedData =>  _cachedNormalizedData ??= Normalize();

    private float[]? _cachedNormalizedData;

    public TimeSpan FromSamplePosition(int position)
    {
        return TimeSpan.FromSeconds((double)position / SamplingRate);
    }

    public int FromTimeSpan(TimeSpan time)
    {
        return (int)(SamplingRate * time.TotalSeconds);
    }

    private float[] Normalize()
    {
        return RawData.Select(x => x / (float)32768.0).ToArray();
    }

    public override string ToString()
    {
        return
            $"RawData Length: {RawData.Length}, Audio Duration: {Duration}, SamplingRate: {SamplingRate}, ChannelCount: {ChannelCount}, BitDepth: {BitDepth}";
    }
}