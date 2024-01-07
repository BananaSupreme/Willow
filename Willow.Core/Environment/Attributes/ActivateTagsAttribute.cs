namespace Willow.Core.Environment.Attributes;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class ActivateTagsAttribute : Attribute
{
    public string ProcessName { get; }
    public string[] Tags { get; }

    public ActivateTagsAttribute(string processName, params string[] tags)
    {
        ProcessName = processName;
        Tags = tags;
    }
}