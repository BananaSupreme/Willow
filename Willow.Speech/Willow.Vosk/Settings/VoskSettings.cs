using Willow.Vosk.Enums;

namespace Willow.Vosk.Settings;

/// <summary>
/// The settings for the VOSK STT, currently only the english variant is supported.
/// </summary>
/// <param name="VoskModel">
/// The size of the model, larger the model the heavier and slower it will be, but also more accurate. defaults to
/// small.
/// </param>
public readonly record struct VoskSettings(VoskModel VoskModel)
{
    public const string VoskFolder = "vosk";

    public VoskSettings() : this(VoskModel.Small)
    {
    }

    public string ModelPath => Path.Combine(Directory.GetCurrentDirectory(), VoskFolder, VoskModel.ToString());
}
