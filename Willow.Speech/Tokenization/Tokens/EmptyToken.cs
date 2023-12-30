using Willow.Speech.Tokenization.Enums;
using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.Tokenization.Tokens;

public sealed record EmptyToken : Token
{
    public override TokenType Type => TokenType.Empty;
}