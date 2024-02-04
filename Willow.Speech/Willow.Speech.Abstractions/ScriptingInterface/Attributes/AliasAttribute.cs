namespace Willow.Speech.ScriptingInterface.Attributes;

/// <summary>
/// Defines alternative invocation phrases for a <see cref="IVoiceCommand" />.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class AliasAttribute : Attribute, IVoiceCommandDescriptor
{
    public AliasAttribute(string tag)
    {
        Aliases = [tag];
    }

    public AliasAttribute(params string[] tags)
    {
        Aliases = tags;
    }

    /// <summary>
    /// Alternative invocation phrases.
    /// </summary>
    public string[] Aliases { get; }
}
