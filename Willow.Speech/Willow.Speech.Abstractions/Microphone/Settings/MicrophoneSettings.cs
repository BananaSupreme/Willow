namespace Willow.Speech.Microphone.Settings;

/// <summary>
/// Settings of the microphone to be used.
/// </summary>
/// <param name="ShouldRecordAudio">
/// Should the microphone be turned on, turning this off will turn the microphone off.
/// </param>
/// <param name="RecordingWindowTimeInMilliseconds">The amount of time each recording should be, default 500ms.</param>
/// <param name="MicrophoneIndex">Index of microphone in the system, default -1.</param>
public readonly record struct MicrophoneSettings(bool ShouldRecordAudio, int RecordingWindowTimeInMilliseconds, int MicrophoneIndex)
{
    /// <inheritdoc cref="MicrophoneSettings" />
    public MicrophoneSettings() : this(true, 200, -1)
    {
    }
}
