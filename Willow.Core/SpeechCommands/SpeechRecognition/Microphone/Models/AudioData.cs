namespace Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Models;

public readonly record struct AudioData(short[] RawData, int SamplingRate, ushort ChannelCount, ushort BitDepth)
{
    public TimeSpan Duration => FromSamplePosition(RawData.Length);

    public TimeSpan FromSamplePosition(int position)
    {
        return TimeSpan.FromSeconds((double)position / SamplingRate);
    }

    public int FromTimeSpan(TimeSpan time)
    {
        return (int)(SamplingRate * time.TotalSeconds);
    }

    public override string ToString()
    {
        return
            $"RawData Length: {RawData.Length}, Audio Duration: {Duration}, SamplingRate: {SamplingRate}, ChannelCount: {ChannelCount}, BitDepth: {BitDepth}";
    }
}