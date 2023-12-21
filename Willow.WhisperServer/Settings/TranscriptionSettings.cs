namespace Willow.WhisperServer.Settings;

[ToString]
public sealed class TranscriptionSettings
{
    private static readonly float[] _temperatureDefaults = [0.0f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f];
    private static readonly int[] _suppressTokensDefaults = [-1];
    public int BeamSize { get; set; } = 5;
    public int BestOf { get; set; } = 5;
    public float Patience { get; set; } = 1.0f;
    public float LengthPenalty { get; set; } = 1.0f;
    public float RepetitionPenalty { get; set; } = 1.0f;
    public int NoRepeatNgramSize { get; set; } = 0;
    public float[] Temperature { get; set; } = _temperatureDefaults;
    public float CompressionRatioThreshold { get; set; } = 2.4f;
    public float LogProbThreshold { get; set; } = -1.0f;
    public float NoSpeechThreshold { get; set; } = 0.6f;
    public bool ConditionOnPreviousText { get; set; } = true;
    public float PromptResetOnTemperature { get; set; } = 0.5f;
    public bool SuppressBlank { get; set; } = true;
    public int[] SuppressTokens { get; set; } = _suppressTokensDefaults;
}