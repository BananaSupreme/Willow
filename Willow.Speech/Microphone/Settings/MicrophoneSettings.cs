namespace Willow.Speech.Microphone.Settings;

public readonly record struct MicrophoneSettings(
    int RecordingWindowTimeInMilliseconds,
    int MicrophoneIndex)
{
    public MicrophoneSettings()
        : this(500, -1)
    {
    }
}