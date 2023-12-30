using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

internal sealed record OneOfNodeProcessor(
    string CaptureName,
    Token[] ValidWords)
    : CapturingNodeProcessor(CaptureName)
{
    public override uint Weight => (uint)ValidWords.Length;

    protected override bool IsTokenMatch(Token token)
    {
        return ValidWords.Contains(token);
    }

    public bool Equals(OneOfNodeProcessor? other)
    {
        return other is not null
               && base.Equals(other) && ValidWords.SequenceEqual(other.ValidWords);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), ValidWords);
    }
}