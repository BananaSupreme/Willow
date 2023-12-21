using Willow.Core.SpeechCommands.Tokenization.Enums;

namespace Willow.Core.SpeechCommands.Tokenization.Exceptions;

public sealed class IncorrectTokenTypeException : InvalidOperationException
{
    public IncorrectTokenTypeException(TokenType expected, Type requested)
        : base($"Incorrect token type. Expected {expected}, requested {requested.Name}")
    {
    }

    public IncorrectTokenTypeException()
    {
    }

    public IncorrectTokenTypeException(string? message) : base(message)
    {
    }

    public IncorrectTokenTypeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}