﻿namespace Willow.Core.SpeechCommands.SpeechRecognition.VAD.Settings;

[ToString]
public sealed class SileroSettings
{
    public float Threshold { get; set; } = 0.3f;
    public int MinSpeechDurationMilliseconds { get; set; } = 30;
    public float MaxSpeechDurationSeconds { get; set; } = float.PositiveInfinity;
    public int MinSilenceDurationMilliseconds { get; set; } = 150;
    public int WindowSizeSamples { get; set; } = 1536;
    public int SpeechPadMilliseconds { get; set; } = 100;
}