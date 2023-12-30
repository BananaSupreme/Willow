using Willow.Core.Environment.Models;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

internal sealed record OptionalNodeProcessor(string FlagName, INodeProcessor InnerNode) : INodeProcessor
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