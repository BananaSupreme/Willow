namespace Willow.Speech.SpeechToText.Enums;

/// <summary>
/// The currently selected engine.
/// </summary>
public enum SelectedSpeechEngine
{
    /// <summary>
    /// Vosk speech engine, light-weight but less accurate engine.
    /// </summary>
    Vosk,

    /// <summary>
    /// Whisper by OpenAI, can be quite heavy but is also very accurate.
    /// </summary>
    Whisper
}