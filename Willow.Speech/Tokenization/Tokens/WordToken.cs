using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.Tokenization.Tokens;

/// <summary>
/// A token representing alphabet based words.
/// </summary>
public sealed record WordToken(string Value) : Token
{
    public override string GetString()
    {
        return Value.ToLower();
    }

    public override string ToString()
    {
        return GetString();
    }

    public bool Equals(WordToken? other)
    {
        return other is not null
               && other.Value.Equals(Value, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }
}