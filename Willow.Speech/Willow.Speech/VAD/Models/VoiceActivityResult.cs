namespace Willow.Speech.VAD.Models;

/// <summary>
/// Result of the speech activity detection.
/// </summary>
/// <param name="IsSpeechDetected">Was any speech detected in the input.</param>
/// <param name="SpeechStart">Speech start time.</param>
/// <param name="SpeechEnd">Speech end time.</param>
public readonly record struct VoiceActivityResult(bool IsSpeechDetected,
                                                  TimeSpan SpeechStart,
                                                  TimeSpan SpeechEnd)
{
    private static readonly VoiceActivityResult _failed = new(false, TimeSpan.Zero, TimeSpan.Zero);

    /// <summary>
    /// Convenience method to create a successful result.
    /// </summary>
    /// <param name="start">Time speech started.</param>
    /// <param name="end">Time speech ended.</param>
    public static VoiceActivityResult Success(TimeSpan start, TimeSpan end)
    {
        return new VoiceActivityResult(true, start, end);
    }

    /// <summary>
    /// Convenience method to create a failed result.
    /// </summary>
    public static VoiceActivityResult Failed()
    {
        return _failed;
    }
}
