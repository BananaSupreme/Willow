using Willow.Environment.Models;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing;

internal sealed class Node
{
    public Node[] Children { get; }
    public INodeProcessor NodeProcessor { get; }
    public TagRequirement[] TagRequirements { get; }

    public Node(Node[] children, INodeProcessor nodeProcessor, TagRequirement[] tagRequirements)
    {
        Children = children;
        NodeProcessor = nodeProcessor;
        TagRequirements = tagRequirements;
    }

    public (CommandBuilder Builder, ReadOnlyMemory<Token> RemainingTokens) ProcessToken(ReadOnlyMemory<Token> tokens,
        CommandBuilder builder,
        Tag[] environmentTags)
    {
        var (isSuccessful, builderResult, remainingTokens) = NodeProcessor.ProcessToken(tokens, builder);
        return isSuccessful ? ProcessChildren(remainingTokens, builderResult, environmentTags) : (builder, tokens);
    }

    private bool IsSatisfyingTagRequirements(Tag[] tags)
    {
        return TagRequirements.Length == 0 || Array.Exists(TagRequirements, r => r.IsSatisfied(tags));
    }

    private (CommandBuilder Builder, ReadOnlyMemory<Token> RemainingTokens) ProcessChildren(
        ReadOnlyMemory<Token> tokens,
        CommandBuilder builder,
        Tag[] environmentTags)
    {
        var children = Children.Where(c => c.IsSatisfyingTagRequirements(environmentTags))
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
        return TagRequirements.Where(x => x.IsSatisfied(tags)).Max(static x => x.Tags.Length);
    }
}
