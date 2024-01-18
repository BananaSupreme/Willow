namespace Willow.Speech.VoiceCommandCompilation.Exceptions;

/// <summary>
/// An error was found in the compilation process, the message should introduce more details.
/// </summary>
public sealed class CommandCompilationException : Exception
{
    public CommandCompilationException()
    {
    }

    public CommandCompilationException(string? message) : base(message)
    {
    }

    public CommandCompilationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
