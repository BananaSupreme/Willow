using Willow.Core.Environment.Models;
using Willow.Core.SpeechCommands.Tokenization.Tokens;
using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Models;

namespace Willow.Core.SpeechCommands.VoiceCommandParsing.NodeProcessors;

internal sealed record OptionalNodeProcessor(INodeProcessor InnerNode, string FlagName) : INodeProcessor
{
    public bool IsLeaf => InnerNode.IsLeaf;
    public uint Weight => InnerNode.Weight;

    public NodeProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder,
                                             Tag[] environmentTags)
    {
        var (isSuccessful, innerBuilder, remainingTokens) = InnerNode.ProcessToken(tokens, builder, environmentTags);
        if (isSuccessful)
        {
            innerBuilder.AddParameter(FlagName, new EmptyToken());
            return new(true, innerBuilder, remainingTokens);
        }

        return new(true, builder, tokens);
    }
}