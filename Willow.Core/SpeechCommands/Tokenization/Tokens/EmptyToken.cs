using Willow.Core.SpeechCommands.Tokenization.Enums;
using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;

namespace Willow.Core.SpeechCommands.Tokenization.Tokens;

public sealed record EmptyToken : Token
{
    public override TokenType Type => TokenType.Empty;
}