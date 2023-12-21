using Willow.Core.SpeechCommands.Tokenization.Enums;
using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;

namespace Willow.Core.SpeechCommands.Tokenization.Tokens;

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