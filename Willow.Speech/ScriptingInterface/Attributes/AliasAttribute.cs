namespace Willow.Speech.ScriptingInterface.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AliasAttribute : Attribute
{
    public string[] Aliases { get; }

    public AliasAttribute(string tag)
    {
        Aliases = [tag];
    }

    public AliasAttribute(params string[] tags)
    {
        Aliases = tags;
    }
}