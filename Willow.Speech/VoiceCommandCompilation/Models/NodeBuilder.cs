using Willow.Core.Environment.Models;
using Willow.Speech.VoiceCommandParsing;
using Willow.Speech.VoiceCommandParsing.Abstractions;

// ReSharper disable UnusedMethodReturnValue.Local

namespace Willow.Speech.VoiceCommandCompilation.Models;

internal sealed class NodeBuilder
{
    private readonly List<NodeBuilder> _children = [];
    private readonly List<TagRequirement> _tagRequirements = [];

    private NodeBuilder() { }

    public INodeProcessor? NodeProcessor { get; private set; }
    public IReadOnlyList<NodeBuilder> Children => _children;

    public static NodeBuilder Create()
    {
        return new();
    }

    public NodeBuilder AddChild(NodeBuilder child)
    {
        _children.Add(child);
        return this;
    }

    public NodeBuilder AddTagRequirements(TagRequirement[] tagRequirements)
    {
        foreach (var tagRequirement in tagRequirements)
        {
            AddTagRequirements(tagRequirement);
        }

        return this;
    }
    
    private NodeBuilder AddTagRequirements(TagRequirement tagRequirement)
    {
        if (!_tagRequirements.Contains(tagRequirement))
        {
            _tagRequirements.Add(tagRequirement);
        }

        return this;
    }

    public NodeBuilder SetNodeProcessor(INodeProcessor nodeProcessor)
    {
        NodeProcessor = nodeProcessor;
        return this;
    }

    public Node Build()
    {
        if (NodeProcessor is null)
        {
            throw new InvalidOperationException();
        }

        var children = _children.OrderBy(x => x.NodeProcessor!.IsLeaf)
                                .ThenBy(x => x.NodeProcessor!.Weight)
                                .Select(x => x.Build()).ToArray();
        return new(children, NodeProcessor, _tagRequirements.ToArray());
    }
}