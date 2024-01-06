using Willow.Speech.Microphone.Models;

namespace Willow.WhisperServer.Models;

public readonly record struct TranscriptionParameters(
    AudioData Audio,
    string? Language = null,
    string InitialPrompt = "",
    string Prefix = "");