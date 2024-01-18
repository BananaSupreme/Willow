using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.Abstractions;

/// <summary>
/// A specialized <see cref="INodeProcessor" /> That captures the token if <see cref="IsTokenMatch" /> returns true.
/// </summary>
/// <param name="CaptureName">The variable name the token should go into.</param>
internal abstract record CapturingNodeProcessor(string CaptureName) : INodeProcessor
{
    public bool IsLeaf => false;
    public abstract uint Weight { get; }

    public virtual TokenProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder)
    {
        if (tokens.Length == 0)
        {
            return new TokenProcessingResult(false, builder, tokens);
        }

        var token = tokens.Span[0];
        var matched = IsTokenMatch(token);
        if (matched)
        {
            builder = builder.AddParameter(CaptureName, token);
            return new TokenProcessingResult(true, builder, tokens[1..]);
        }

        return new TokenProcessingResult(false, builder, tokens);
    }

    protected abstract bool IsTokenMatch(Token token);
}
