namespace Willow.WhisperServer.Enum;

/// <summary>
/// The size of the model to use, larger is more expensive but more accurate, size above Small are not really
/// recommended as they are too large to run locally unless the work station is very powerful.
/// </summary>
public enum ModelSize
{
    Tiny,
    Base,
    Small,
    Medium,
    LargeV1,
    LargeV2
}