using Willow.Speech.ScriptingInterface.Abstractions;

namespace Willow.Speech.ScriptingInterface.Attributes;

/// <summary>
/// Defines alternative invocation phrases for a <see cref="IVoiceCommand"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AliasAttribute : Attribute
{
    /// <summary>
    /// Alternative invocation phrases.
    /// </summary>
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