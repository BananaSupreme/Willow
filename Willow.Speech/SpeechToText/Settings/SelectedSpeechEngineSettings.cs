using Willow.Speech.SpeechToText.Enums;

namespace Willow.Speech.SpeechToText.Settings;

/// <summary>
/// The settings of the engine switcher.
/// </summary>
/// <param name="SelectedSpeechEngine">Currently selected engine, default VOSK.</param>
internal readonly record struct SelectedSpeechEngineSettings(SelectedSpeechEngine SelectedSpeechEngine)
{
    /// <inheritdoc cref="SelectedSpeechEngineSettings"/>
    public SelectedSpeechEngineSettings() : this(SelectedSpeechEngine.Vosk)
    {
        
    }
}