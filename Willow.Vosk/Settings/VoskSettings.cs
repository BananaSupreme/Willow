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
    public VoskSettings() : this(VoskModel.Small)
    {
    }

    public const string VoskFolder = $"./vosk";
    public string ModelPath => Path.Combine(VoskFolder, VoskModel.ToString());
}