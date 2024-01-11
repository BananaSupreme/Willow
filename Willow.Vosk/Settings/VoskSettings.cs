using Willow.Vosk.Enums;

namespace Willow.Vosk.Settings;

public readonly record struct VoskSettings(VoskModel VoskModel)
{
    public const string VoskFolder = $"./vosk";
    public string ModelPath => Path.Combine(VoskFolder, VoskModel.ToString());
}