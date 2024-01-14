using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

/// <summary>
/// An empty processor that always succeeds.
/// </summary>
internal sealed record EmptyNodeProcessor : INodeProcessor
{
    public bool IsLeaf => false;
    public uint Weight => uint.MaxValue;

    public TokenProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder)
    {
        return new(true, builder, tokens);
    }
}