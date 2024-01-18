namespace Willow.Core.Environment.Models;

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
    public bool Equals(TagRequirement other)
    {
        return Tags.SequenceEqual(other.Tags);
    }

    public bool IsSatisfied(Tag[] tags)
    {
        return tags.Length == 0 || Array.TrueForAll(Tags, tags.Contains);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Tags);
    }
}
