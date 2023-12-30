using Willow.Core.Environment.Models;
using Willow.Speech.Tokenization.Enums;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

internal sealed record WordNodeProcessor(WordToken Value) : INodeProcessor
{
    public bool IsLeaf => false;
    public uint Weight => 1;

    public NodeProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder,
                                             Tag[] environmentTags)
    {
        return !tokens.IsEmpty && IsTokenMatch(tokens.Span[0])
                   ? new(true, builder, tokens[1..])
                   : new(false, builder, tokens);
    }

    private bool IsTokenMatch(Token token)
    {
        return token.Type == TokenType.Word
               && (WordToken)token == Value;
    }
}