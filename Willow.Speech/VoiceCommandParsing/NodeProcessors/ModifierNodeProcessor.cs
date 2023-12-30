using Willow.Core.Environment.Models;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

internal sealed record ModifierNodeProcessor(Func<CommandBuilder, CommandBuilder> ModifyFunction)
    : INodeProcessor
{
    public bool IsLeaf => true;
    public uint Weight => uint.MaxValue;

    public NodeProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder,
                                             Tag[] environmentTags)
    {
        var newBuilder = ModifyFunction(builder);
        return new(true, newBuilder, tokens);
    }
}