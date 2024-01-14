using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

/// <summary>
/// A processor that succeeds whenever any of the inputted words fits the word spoken.
/// </summary>
/// <param name="CaptureName">The variable name in the command parameters to capture the token.</param>
/// <param name="ValidWords">The words that should be said to consider the operation a success.</param>
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