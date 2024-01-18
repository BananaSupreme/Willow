using Willow.Helpers.Logging.Loggers;

namespace Willow.Core.Privacy.Settings;

/// <summary>
/// The privacy parameters, implementors must look here whenever logging or saving any personal info.
/// </summary>
/// <remarks>
/// When looking at those values implementors should also consider whether the information can be inferred from the logs.
/// </remarks>
/// <param name="AllowLoggingTranscriptions">Whether transcriptions can be logged.</param>
/// <param name="AllowLoggingCommands">Whether the commands captured by the system can be logged.</param>
/// <param name="AllowLoggingActiveWindow">Whether the foreground window can be logged.</param>
/// <seealso cref="RedactingLogger{T}" />
public readonly record struct PrivacySettings(bool AllowLoggingTranscriptions,
                                              bool AllowLoggingCommands,
                                              bool AllowLoggingActiveWindow)
{
    /// <inheritdoc cref="PrivacySettings" />
    public PrivacySettings() : this(true, true, true)
    {
    }
}
