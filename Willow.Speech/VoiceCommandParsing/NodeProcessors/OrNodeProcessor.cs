using Willow.Core.Environment.Models;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

internal sealed record OrNodeProcessor(string SuccessIndexName, INodeProcessor[] InnerNodes) : INodeProcessor
{
    public bool IsLeaf => false;
    public uint Weight => InnerNodes.Min(x => x.Weight);

    public NodeProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder,
                                             Tag[] environmentTags)
    {
        var i = 0;
        foreach (var innerNode in InnerNodes)
        {
            var (isSuccessful, innerBuilder, remainingTokens) =
                innerNode.ProcessToken(tokens, builder, environmentTags);
            
            if (isSuccessful)
            {
                innerBuilder.AddParameter(SuccessIndexName, new NumberToken(i));
                return new(true, innerBuilder, remainingTokens);
            }

            i++;
        }

        return new(false, builder, tokens);
    }
    
    public bool Equals(OrNodeProcessor? other)
    {
        return other is not null
               && SuccessIndexName.Equals(other.SuccessIndexName)
               && InnerNodes.SequenceEqual(other.InnerNodes);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SuccessIndexName, InnerNodes);
    }
}