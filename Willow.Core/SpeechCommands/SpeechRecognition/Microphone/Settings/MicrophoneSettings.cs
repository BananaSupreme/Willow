namespace Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Settings;

[ToString]
public sealed class MicrophoneSettings
{
    public int RecordingWindowTimeInMilliseconds { get; set; } = 300;
    public int MicrophoneIndex { get; set; } = 1;
}