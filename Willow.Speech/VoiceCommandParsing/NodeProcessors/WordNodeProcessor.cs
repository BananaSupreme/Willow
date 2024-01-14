using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

/// <summary>
/// A processor that succeeds when the input token matches a specific word.
/// </summary>
/// <param name="Value">The word to match.</param>
internal sealed record WordNodeProcessor(WordToken Value) : INodeProcessor
{
    public bool IsLeaf => false;
    public uint Weight => 1;

    public TokenProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder)
    {
        return !tokens.IsEmpty && IsTokenMatch(tokens.Span[0])
                   ? new(true, builder, tokens[1..])
                   : new(false, builder, tokens);
    }

    private bool IsTokenMatch(Token token)
    {
        return token is WordToken wordToken
               && wordToken == Value;
    }
}