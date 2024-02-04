namespace Willow.Speech.ScriptingInterface.Attributes;

/// <summary>
/// The display name to be used by the command,
/// </summary>
/// <remarks>
/// If not defined the name will be the title case equivalent of the name of the command without, "VoiceCommand"
/// or "Command".
/// </remarks>
/// <example>
/// "IAmAVoiceCommand" -> "I Am A". <br/>
/// "AnotherCommandInTheSystem" -> "Another Command In The System". <br/>
/// "FinalThatEndsWithCommand" -> "Final That Ends With".
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class NameAttribute : Attribute, IVoiceCommandDescriptor
{
    public NameAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; }
}
