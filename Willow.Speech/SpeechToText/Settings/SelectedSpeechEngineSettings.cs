namespace Willow.Speech.SpeechToText.Settings;

/// <summary>
/// The settings of the engine switcher.
/// </summary>
/// <param name="SelectedSpeechEngine">Currently selected engine.</param>
internal readonly record struct SelectedSpeechEngineSettings(string? SelectedSpeechEngine)
{
    /// <inheritdoc cref="SelectedSpeechEngineSettings" />
    public SelectedSpeechEngineSettings() : this(null)
    {
    }
}
