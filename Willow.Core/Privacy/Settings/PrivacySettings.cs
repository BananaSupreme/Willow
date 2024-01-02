namespace Willow.Core.Privacy.Settings;

[ToString]
public sealed class PrivacySettings
{
    public bool AllowLoggingTranscriptions { get; set; } = true;
    public bool AllowLoggingCommands { get; set; } = true;
    public bool AllowLoggingActiveWindow { get; set; } = true;
}