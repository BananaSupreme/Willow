using Willow.Core.Environment.Models;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

internal sealed record EmptyNodeProcessor : INodeProcessor
{
    public bool IsLeaf => false;
    public uint Weight => uint.MaxValue;

    public NodeProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder,
                                             Tag[] environmentTags)
    {
        return new(true, builder, tokens);
    }
}