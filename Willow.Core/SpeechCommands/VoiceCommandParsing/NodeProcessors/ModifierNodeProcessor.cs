using Willow.Core.Environment.Models;
using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Models;

namespace Willow.Core.SpeechCommands.VoiceCommandParsing.NodeProcessors;

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