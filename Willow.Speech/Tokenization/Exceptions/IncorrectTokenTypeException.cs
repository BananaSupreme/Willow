namespace Willow.Speech.Tokenization.Exceptions;

public sealed class IncorrectTokenTypeException : InvalidOperationException
{
    public IncorrectTokenTypeException(Type expected, Type requested) : base(
        $"Incorrect token type. Expected {expected.Name}, requested {requested.Name}")
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
