using Willow.WhisperServer.Enum;

namespace Willow.WhisperServer.Settings;

[ToString]
public sealed class WhisperModelSettings
{
    public ModelSize ModelSize { get; set; } = ModelSize.Tiny;
    public bool EnglishOnly { get; set; } = true;
    public DeviceType Device { get; set; } = DeviceType.Auto;
    public int[] DeviceIndex { get; set; } = [];
    public ComputeType ComputeType { get; set; } = ComputeType.Int8;
    public int CpuThreads { get; init; } = 1;
}