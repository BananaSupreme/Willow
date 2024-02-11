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
        var tagCount = TagRequirements.Where(x => x.IsSatisfied(tags)).Max(static x => x.Tags.Length);

        //Basically everyone should have at least one tag defined which is the activation mode unless any activation mode
        //fits, in which case simply by having an activation mode a command overrides previous commands, which completely
        //breaks in dictation mode for example (as mode changing has no activation, so we need to make sure everyone has
        //at least one tag, because if you don't care about activation mode, does not mean the tag shouldn't "exist"
        return Math.Max(tagCount, 1);
    }
}
