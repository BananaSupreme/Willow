using Willow.Speech.Tokenization.Enums;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

internal sealed record NumberNodeProcessor(string CaptureName) : CapturingNodeProcessor(CaptureName)
{
    public override uint Weight => 1;

    protected override bool IsTokenMatch(Token token)
    {
        return token.Type == TokenType.Number;
    }
}