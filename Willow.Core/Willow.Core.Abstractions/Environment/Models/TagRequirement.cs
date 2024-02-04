namespace Willow.Environment.Models;

/// <summary>
/// Represents a collection of tags associated with a requirement. <br/>
/// For example a <b>Find</b> command requires the user to be in an environment where finding makes sense
/// This might be an IDE, the browser or file explorer, all of which are valid.
/// This is how the tags are grouped together to identify if one of the groups in their entirety
/// fits the current environment
/// </summary>
/// <remarks>
/// An empty tag group means that the tag is valid for any environment
/// </remarks>
/// <param name="Tags">All the tags that must be satisfied when </param>
public readonly record struct TagRequirement(Tag[] Tags)
{
    public static readonly TagRequirement Empty = new([]);

    public bool Equals(TagRequirement other)
    {
        return Tags.SequenceEqual(other.Tags);
    }

    /// <summary>
    /// Test whether environment contains all the tags required in this group.
    /// </summary>
    /// <param name="tags">The tags in the environment.</param>
    /// <returns>Whether the environment satisfies the requirements in the tags.</returns>
    public bool IsSatisfied(Tag[] tags)
    {
        return Tags.Length == 0 || Array.TrueForAll(Tags, tags.Contains);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Tags);
    }
}
