namespace Willow.Speech.ScriptingInterface.Attributes;

public sealed class NameAttribute : Attribute
{
    public string Name { get; }

    public NameAttribute(string name)
    {
        Name = name;
    }
}