using Willow.Speech.Tokenization.Enums;
using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.Tokenization.Tokens;

public sealed record NumberToken(int Value) : Token
{
    public override TokenType Type => TokenType.Number;

    public override string GetString()
    {
        return Value.ToString();
    }

    public override int GetInt32()
    {
        return Value;
    }
}