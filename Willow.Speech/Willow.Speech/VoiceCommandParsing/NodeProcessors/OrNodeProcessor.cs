using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

/// <summary>
/// A processor that succeeds whenever any of the inner processors succeeds.
/// </summary>
/// <param name="SuccessIndexName">
/// The variable name in the command parameters to store the index of the successful
/// item.
/// </param>
/// <param name="InnerNodes">The nodes to test against.</param>
internal sealed record OrNodeProcessor(string SuccessIndexName, INodeProcessor[] InnerNodes) : INodeProcessor
{
    public bool Equals(OrNodeProcessor? other)
    {
        return other is not null
               && SuccessIndexName.Equals(other.SuccessIndexName)
               && InnerNodes.SequenceEqual(other.InnerNodes);
    }

    public bool IsLeaf => false;
    public uint Weight => InnerNodes.Min(static x => x.Weight);

    public TokenProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder)
    {
        var i = 0;
        foreach (var innerNode in InnerNodes)
        {
            var (isSuccessful, innerBuilder, remainingTokens) = innerNode.ProcessToken(tokens, builder);

            if (isSuccessful)
            {
                innerBuilder.AddParameter(SuccessIndexName, new NumberToken(i));
                return new TokenProcessingResult(true, innerBuilder, remainingTokens);
            }

            i++;
        }

        return new TokenProcessingResult(false, builder, tokens);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SuccessIndexName, InnerNodes);
    }
}
