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
        return !tokens.IsEmpty && tokens.Span[0].Match(Value)
                   ? new TokenProcessingResult(true, builder, tokens[1..])
                   : new TokenProcessingResult(false, builder, tokens);
    }
}
