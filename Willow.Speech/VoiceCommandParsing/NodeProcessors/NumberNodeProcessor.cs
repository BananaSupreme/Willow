using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

/// <summary>
/// A processor that succeeds when the input is a number token.
/// </summary>
/// <param name="CaptureName">The variable name in the command parameters to capture the token.</param>
internal sealed record NumberNodeProcessor(string CaptureName) : CapturingNodeProcessor(CaptureName)
{
    public override uint Weight => 1;

    protected override bool IsTokenMatch(Token token)
    {
        return token is NumberToken;
    }
}