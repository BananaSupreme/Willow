using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.Tokenization.Tokens;

/// <summary>
/// A token representing alphabet based words.
/// </summary>
public record WordToken(string Value) : Token
{
    private readonly string _lowered = Value.ToLower();

    public virtual bool Equals(WordToken? other)
    {
        return other is not null && other.Value.Equals(Value, StringComparison.OrdinalIgnoreCase);
    }

    public override string GetString()
    {
        return _lowered;
    }

    public override string ToString()
    {
        return GetString();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }
}
