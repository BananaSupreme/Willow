using System.Diagnostics.CodeAnalysis;

using Willow.Speech.Tokenization.Exceptions;

namespace Willow.Speech.Tokenization.Tokens.Abstractions;

public abstract record Token
{
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

    public virtual bool Match(Token other)
    {
        return this.Equals(other);
    }

    [DoesNotReturn]
    private void ThrowInvalid(Type requested)
    {
        throw new IncorrectTokenTypeException(this.GetType(), requested);
    }
}