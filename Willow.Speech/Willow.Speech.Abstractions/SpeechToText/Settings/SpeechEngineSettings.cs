namespace Willow.Speech.SpeechToText.Settings;

/// <summary>
/// The settings of the engine switcher.
/// </summary>
/// <param name="SelectedSpeechEngine">Currently selected engine.</param>
public readonly record struct SpeechEngineSettings(string? SelectedSpeechEngine)
{
    /// <inheritdoc cref="SpeechEngineSettings" />
    public SpeechEngineSettings() : this(null)
    {
    }
}
