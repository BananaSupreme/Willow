using Willow.WhisperServer.Enum;

namespace Willow.WhisperServer.Settings;

/// <summary>
/// The settings of the Whisper model itself.
/// </summary>
/// <param name="ModelSize">
/// The size of the model to use. Defaults to "Base"
/// </param>
/// <param name="EnglishOnly">
/// Whether to use the english only version of the model or the multilingual version. Defaults to true.
/// </param>
/// <param name="Device">
/// The device to use for computation. Defaults to "Auto" which tries to detect if GPU is available.
/// </param>
/// <param name="DeviceIndex">
/// The index of the device to use, if more than one device of the type are available. Defaults to 0.
/// </param>
/// <param name="ComputeType">
/// </param>
/// <param name="CpuThreads">Number of threads to use when running on the machine, defaults to 4</param>
public readonly record struct WhisperModelSettings(ModelSize ModelSize,
                                                   bool EnglishOnly,
                                                   DeviceType Device,
                                                   int[] DeviceIndex,
                                                   ComputeType ComputeType,
                                                   int CpuThreads)
{
    public WhisperModelSettings() : this(ModelSize.Small, true, DeviceType.Auto, [], ComputeType.Default, 4)
    {
    }
}
