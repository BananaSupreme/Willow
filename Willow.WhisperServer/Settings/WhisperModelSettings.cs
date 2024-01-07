using Willow.WhisperServer.Enum;

namespace Willow.WhisperServer.Settings;

public readonly record struct WhisperModelSettings(
    ModelSize ModelSize,
    bool EnglishOnly,
    DeviceType Device,
    int[] DeviceIndex,
    ComputeType ComputeType,
    int CpuThreads)
{
    public WhisperModelSettings()
        : this(ModelSize.Tiny,
            true,
            DeviceType.Auto,
            [],
            ComputeType.Default,
            1)
    {
    }
}