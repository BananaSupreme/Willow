namespace Willow.Core.CommandExecution.Exceptions;

public class CommandNotFoundException : InvalidOperationException
{
    public CommandNotFoundException(Guid id)
        : base($"Tried to locate and failed command {id})")
    {
    }

    public CommandNotFoundException()
    {
    }

    public CommandNotFoundException(string? message) : base(message)
    {
    }

    public CommandNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}