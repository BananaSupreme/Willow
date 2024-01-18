namespace Willow.WhisperServer.Enum;

/// <summary>
/// Specifies the type of computation to be used during the transcription process.
/// </summary>
/// <remarks>
/// The choice of ComputeType can have a significant impact on the performance and accuracy of the transcription.
/// Different compute types may use different levels of precision, which can affect the speed and resource usage of the
/// transcription process.
/// <list type="bullet">
/// <item>
/// <b>Higher precision types (e.g., Float32)</b>: Provide more accurate transcription results but may require
/// more
/// computational resources and time.
/// </item>
/// <item>
/// <b>Lower precision types (e.g., Int16)</b>: Can speed up the transcription process and reduce resource
/// usage but
/// might lead to slightly less accurate results.
/// </item>
/// </list>
/// </remarks>
public enum ComputeType
{
    /// <summary>
    /// The default computation type. Currently Float32.
    /// </summary>
    Default,

    /// <summary>
    /// Uses 32-bit floating-point numbers for computation. Offers high precision, suitable for scenarios where
    /// transcription accuracy is critical.
    /// </summary>
    Float32,

    /// <summary>
    /// Employs 16-bit floating-point numbers. It's a middle ground, providing a balance between accuracy and
    /// computational efficiency.
    /// </summary>
    Float16,

    /// <summary>
    /// Utilizes 8-bit integer values. This type is highly efficient in terms of computation and resource usage, but it
    /// may compromise on the precision of the transcription.
    /// </summary>
    Int8
}
