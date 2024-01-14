using Willow.Core.Environment.Models;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing;

internal sealed class Node
{
    private readonly Node[] _children;
    private readonly INodeProcessor _nodeProcessor;
    private readonly TagRequirement[] _tagRequirements;

    public Node(Node[] children, INodeProcessor nodeProcessor, TagRequirement[] tagRequirements)
    {
        _children = children;
        _nodeProcessor = nodeProcessor;
        _tagRequirements = tagRequirements;
    }

    public (CommandBuilder Builder, ReadOnlyMemory<Token> RemainingTokens) ProcessToken(
        ReadOnlyMemory<Token> tokens, CommandBuilder builder, Tag[] environmentTags)
    {
        var (isSuccessful, builderResult, remainingTokens) =
            _nodeProcessor.ProcessToken(tokens, builder);
        return isSuccessful
                   ? ProcessChildren(remainingTokens, builderResult, environmentTags)
                   : (builder, tokens);
    }

    private bool IsSatisfyingTagRequirements(Tag[] tags)
    {
        return _tagRequirements.Length == 0 || Array.Exists(_tagRequirements, r => r.IsSatisfied(tags));
    }

    private (CommandBuilder Builder, ReadOnlyMemory<Token> RemainingTokens) ProcessChildren(
        ReadOnlyMemory<Token> tokens, CommandBuilder builder, Tag[] environmentTags)
    {
        var children = _children.Where(c => c.IsSatisfyingTagRequirements(environmentTags))
                                .OrderByDescending(c => c.GetMostSpecificTagsMatchedCount(environmentTags));

        foreach (var child in children)
        {
            var (builderResult, remainingTokens) = child.ProcessToken(tokens, builder, environmentTags);
            if (builderResult.IsSuccessful)
            {
                return (builderResult, remainingTokens);
            }
        }

        return (builder, tokens);
    }

    private int GetMostSpecificTagsMatchedCount(Tag[] tags)
    {
        return _tagRequirements.Where(x => x.IsSatisfied(tags)).Max(x => x.Tags.Length);
    }
}