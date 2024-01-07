using Willow.Core.Environment.Models;

namespace Willow.Speech.ScriptingInterface.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class TagAttribute : Attribute
{
    public Tag[] Tags { get; }

    public TagAttribute()
    {
        Tags = [];
    }
    
    public TagAttribute(string tag)
    {
        Tags = [new(tag)];
    }

    public TagAttribute(params string[] tags)
    {
        Tags = tags.Select(x => new Tag(x)).ToArray();
    }
}