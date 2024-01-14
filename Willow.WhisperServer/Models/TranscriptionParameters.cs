using Willow.Speech.Microphone.Models;

namespace Willow.WhisperServer.Models;

/// <summary>
/// The parameters to transfer to the Whisper voice system.
/// </summary>
/// <param name="Audio">The audio data.</param>
/// <param name="Language">The expected language, defaults null.</param>
/// <param name="InitialPrompt">A prompt to transfer into whisper.</param>
/// <param name="Prefix">A prefix to be added into the transcription.</param>
public readonly record struct TranscriptionParameters(
    AudioData Audio,
    string? Language = null,
    string InitialPrompt = "",
    string Prefix = "");