namespace Willow.Core.Environment.Attributes;

[AttributeUsage(AttributeTargets.Assembly)]
public class ActivateTagsAttribute : Attribute
{
    public string ProcessName { get; }
    public string[] Tags { get; }

    public ActivateTagsAttribute(string processName, params string[] tags)
    {
        ProcessName = processName;
        Tags = tags;
    }
}