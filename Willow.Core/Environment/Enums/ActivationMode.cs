namespace Willow.Core.Environment.Enums;

/// <summary>
/// The mode which the system works under.
/// </summary>
public enum ActivationMode
{
    /// <summary>
    /// The system looks for commands only and builds actions upon them.
    /// </summary>
    Command,

    /// <summary>
    /// The system is geared towards dictation, any item that doesn't fit dictation command is written out verbatim.
    /// </summary>
    Dictation
}
