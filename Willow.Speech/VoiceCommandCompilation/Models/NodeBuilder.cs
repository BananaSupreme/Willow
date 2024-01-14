using Willow.Core.Environment.Models;
using Willow.Speech.VoiceCommandCompilation.Exceptions;
using Willow.Speech.VoiceCommandParsing;
using Willow.Speech.VoiceCommandParsing.Abstractions;

// ReSharper disable UnusedMethodReturnValue.Local

namespace Willow.Speech.VoiceCommandCompilation.Models;

/// <summary>
/// A builder for the <see cref="Node"/> type.
/// </summary>
internal sealed class NodeBuilder
{
    private readonly List<NodeBuilder> _children = [];
    private readonly List<TagRequirement> _tagRequirements = [];

    private NodeBuilder() { }

    /// <summary>
    /// The processor associated with the node.
    /// </summary>
    public INodeProcessor? NodeProcessor { get; private set; }

    /// <summary>
    /// Children of the node, represented as builders.
    /// </summary>
    public IReadOnlyList<NodeBuilder> Children => _children;

    /// <summary>
    /// Creates a new instance of the builder.
    /// </summary>
    /// <returns>A fresh instance of the builder.</returns>
    public static NodeBuilder Create()
    {
        return new();
    }

    /// <summary>
    /// Adds a new child node to this builder, represented as a builder so it can also be expended upon.
    /// </summary>
    /// <param name="child">Child builder.</param>
    public NodeBuilder AddChild(NodeBuilder child)
    {
        _children.Add(child);
        return this;
    }

    /// <summary>
    /// Add a new requirement for the tags to be present to the node, if the environment fulfills any of those
    /// requirements the node can be processed.
    /// </summary>
    /// <param name="tagRequirements">The tag requirements to add.</param>
    public NodeBuilder AddTagRequirements(TagRequirement[] tagRequirements)
    {
        foreach (var tagRequirement in tagRequirements)
        {
            AddTagRequirements(tagRequirement);
        }

        return this;
    }

    /// <inheritdoc cref="AddTagRequirements"/>
    private NodeBuilder AddTagRequirements(TagRequirement tagRequirement)
    {
        if (!_tagRequirements.Contains(tagRequirement))
        {
            _tagRequirements.Add(tagRequirement);
        }

        return this;
    }

    /// <summary>
    /// Sets the processor for the node.
    /// </summary>
    /// <param name="nodeProcessor">The processor the node should use internally.</param>
    public NodeBuilder SetNodeProcessor(INodeProcessor nodeProcessor)
    {
        NodeProcessor = nodeProcessor;
        return this;
    }

    /// <summary>
    /// Builds the final node and all its children sorted by their weight.
    /// </summary>
    /// <returns>A fully built node with all its children built as well.</returns>
    /// <exception cref="InvalidOperationException">
    /// A node building request was made but the processor was never set with <see cref="SetNodeProcessor"/>.
    /// </exception>
    public Node Build()
    {
        if (NodeProcessor is null)
        {
            throw new NodeMissingNodeProcessorException();
        }

        var children = _children.OrderBy(x => x.NodeProcessor!.IsLeaf)
                                .ThenBy(x => x.NodeProcessor!.Weight)
                                .Select(x => x.Build()).ToArray();
        return new(children, NodeProcessor, _tagRequirements.ToArray());
    }
}