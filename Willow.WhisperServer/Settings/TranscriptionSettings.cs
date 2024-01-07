namespace Willow.WhisperServer.Settings;

public readonly record struct TranscriptionSettings(
    int BeamSize,
    int BestOf,
    float Patience,
    float LengthPenalty,
    float RepetitionPenalty,
    int NoRepeatNgramSize,
    float[] Temperature,
    float CompressionRatioThreshold,
    float LogProbThreshold,
    float NoSpeechThreshold,
    bool ConditionOnPreviousText,
    float PromptResetOnTemperature,
    bool SuppressBlank,
    int[] SuppressTokens)
{
    private static readonly float[] _temperatureDefaults = [0.0f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f];
    private static readonly int[] _suppressTokensDefaults = [-1];

    public TranscriptionSettings()
        : this(
            5,
            5,
            1.0f,
            1.0f,
            1.0f,
            0,
            _temperatureDefaults,
            2.4f,
            -1.0f,
            0.6f,
            true,
            0.5f,
            true,
            _suppressTokensDefaults)
    {
    }
}