namespace Willow.Speech.VAD.Models;

public readonly record struct VoiceActivityResult(bool IsSpeechDetected, TimeSpan SpeechStart, TimeSpan SpeechEnd)
{
    private static readonly VoiceActivityResult _failed = new(false, TimeSpan.Zero, TimeSpan.Zero);

    public static VoiceActivityResult Success(TimeSpan start, TimeSpan end)
    {
        return new(true, start, end);
    }

    public static VoiceActivityResult Failed()
    {
        return _failed;
    }
}