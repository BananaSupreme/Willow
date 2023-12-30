namespace Willow.Speech.VoiceCommandCompilation.Exceptions;

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