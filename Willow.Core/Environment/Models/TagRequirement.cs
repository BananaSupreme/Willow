namespace Willow.Core.Environment.Models;

public readonly record struct TagRequirement(Tag[] Tags)
{
    public bool IsSatisfied(Tag[] tags)
    {
        return tags.Length == 0 || Array.TrueForAll(Tags, tags.Contains);
    }

    public bool Equals(TagRequirement other)
    {
        return Tags.SequenceEqual(other.Tags);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Tags);
    }
}