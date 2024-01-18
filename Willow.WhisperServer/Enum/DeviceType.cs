namespace Willow.WhisperServer.Enum;

/// <summary>
/// The type of device to perform the computation on.
/// </summary>
public enum DeviceType
{
    /// <summary>
    /// Perform on the CPU.
    /// </summary>
    Cpu,

    /// <summary>
    /// Perform on a CUDA enabled GPU.
    /// </summary>
    Cuda,

    /// <summary>
    /// Will select if detects a CUDA enabled GPU and all the correct dependencies.
    /// </summary>
    Auto
}
