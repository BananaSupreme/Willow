namespace Willow.Speech.ScriptingInterface.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class DescriptionAttribute : Attribute
{
    public string Description { get; }

    public DescriptionAttribute(string tag)
    {
        Description = tag;
    }
}