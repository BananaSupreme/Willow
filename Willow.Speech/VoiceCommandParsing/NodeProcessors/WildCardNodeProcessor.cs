using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

internal sealed record WildCardNodeProcessor(string CaptureName)
    : CapturingNodeProcessor(CaptureName)
{
    public override uint Weight => uint.MaxValue - 1;

    protected override bool IsTokenMatch(Token token)
    {
        return true;
    }
}