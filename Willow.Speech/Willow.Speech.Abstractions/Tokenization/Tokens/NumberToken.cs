using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.Tokenization.Tokens;

/// <summary>
/// A token representing a value.
/// </summary>
public sealed record NumberToken(int Value) : Token
{
    public override string GetString()
    {
        return Value.ToString();
    }

    public override int GetInt32()
    {
        return Value;
    }
}
