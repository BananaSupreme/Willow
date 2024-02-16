namespace Willow.Speech.VAD.Settings;

public readonly record struct BufferingSettings(int VadFalseTolerance,
                                                int AcceptedSamplingRate,
                                                int MaxSeconds)
{
    public BufferingSettings() : this(VadFalseTolerance: 2, AcceptedSamplingRate: 16000, MaxSeconds: 10)
    {
    }
}
