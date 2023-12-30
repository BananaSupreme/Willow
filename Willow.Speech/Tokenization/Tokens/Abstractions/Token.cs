using System.Diagnostics.CodeAnalysis;

using Willow.Speech.Tokenization.Enums;
using Willow.Speech.Tokenization.Exceptions;

namespace Willow.Speech.Tokenization.Tokens.Abstractions;

public abstract record Token
{
    public abstract TokenType Type { get; }

    public virtual string GetString()
    {
        ThrowInvalid(typeof(string));
        return string.Empty;
    }

    public virtual int GetInt32()
    {
        ThrowInvalid(typeof(int));
        return 0;
    }

    [DoesNotReturn]
    private void ThrowInvalid(Type requested)
    {
        throw new IncorrectTokenTypeException(Type, requested);
    }
}