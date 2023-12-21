using Willow.Core.SpeechCommands.Tokenization.Enums;
using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;

namespace Willow.Core.SpeechCommands.VoiceCommandParsing.NodeProcessors;

internal sealed record NumberNodeProcessor(string CaptureName) : CapturingNodeProcessor(CaptureName)
{
    public override uint Weight => 1;

    protected override bool IsTokenMatch(Token token)
    {
        return token.Type == TokenType.Number;
    }
}