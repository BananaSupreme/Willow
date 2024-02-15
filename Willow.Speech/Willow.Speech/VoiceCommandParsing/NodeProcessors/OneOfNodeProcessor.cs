using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

/// <summary>
/// A processor that succeeds whenever any of the inputted words fits the word spoken.
/// </summary>
/// <param name="CaptureName">The variable name in the command parameters to capture the token.</param>
/// <param name="ValidWords">The words that should be said to consider the operation a success.</param>
internal sealed record OneOfNodeProcessor(string CaptureName, Token[] ValidWords) : INodeProcessor
{
    public bool IsLeaf => false;
    public uint Weight => (uint)ValidWords.Length;

    public TokenProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder)
    {
        if (tokens.Length == 0)
        {
            return new TokenProcessingResult(false, builder, tokens);
        }

        var matched = Array.Find(ValidWords, t => Match(tokens, t));
        if (matched is null)
        {
            return new TokenProcessingResult(false, builder, tokens);
        }

        builder = builder.AddParameter(CaptureName, new WordToken(matched.GetString()));
        return new TokenProcessingResult(true, builder, tokens[1..]);
    }

    private static bool Match(ReadOnlyMemory<Token> tokens, Token t)
    {
        return t switch
        {
            MergedToken merged => tokens.Length >= merged.Tokens.Length && merged.Match(tokens[..tokens.Length]),
            _ => tokens.Span[0].Match(t)
        };
    }

    public bool Equals(OneOfNodeProcessor? other)
    {
        return other is not null && CaptureName.Equals(other.CaptureName) && ValidWords.SequenceEqual(other.ValidWords);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CaptureName, ValidWords);
    }
}
