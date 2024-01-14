﻿using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

/// <summary>
/// Processes an input that should match all the processors as processed one by one, otherwise fails the entire thing.
/// </summary>
/// <param name="InnerNodes">All the processors that should succeed.</param>
internal sealed record AndNodeProcessor(INodeProcessor[] InnerNodes) : INodeProcessor
{
    public bool IsLeaf => false;
    public uint Weight => CalculateWeight();

    public TokenProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder)
    {
        var remainingTokens = tokens;
        var innerBuilder = builder;
        foreach (var innerNode in InnerNodes)
        {
            (var isSuccessful, innerBuilder, remainingTokens) =
                innerNode.ProcessToken(remainingTokens, innerBuilder);

            if (!isSuccessful)
            {
                return new(false, builder, remainingTokens);
            }
        }

        return new(true, innerBuilder, remainingTokens);
    }

    private uint CalculateWeight()
    {
        var summedWeight = (uint)InnerNodes.Sum(x => x.Weight);
        var maxWeight = InnerNodes.Max(x => x.Weight);
        return Math.Max(summedWeight, maxWeight);
    }

    public bool Equals(AndNodeProcessor? other)
    {
        return other is not null
               && InnerNodes.SequenceEqual(other.InnerNodes);
    }

    public override int GetHashCode()
    {
        return InnerNodes.GetHashCode();
    }
}