namespace Willow.Core.Privacy.Settings;

public readonly record struct PrivacySettings(
    bool AllowLoggingTranscriptions,
    bool AllowLoggingCommands,
    bool AllowLoggingActiveWindow)
{
    public PrivacySettings() 
        : this(true, true, true)
    {
    }
}