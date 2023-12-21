using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;

namespace Willow.Core.SpeechCommands.VoiceCommandParsing.NodeProcessors;

internal sealed record WildCardNodeProcessor(string CaptureName)
    : CapturingNodeProcessor(CaptureName)
{
    public override uint Weight => uint.MaxValue - 1;

    protected override bool IsTokenMatch(Token token)
    {
        return true;
    }
}