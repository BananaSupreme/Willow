namespace Willow.Speech.ScriptingInterface.Attributes;

/// <summary>
/// Defines a user readable description of the <see cref="IVoiceCommand" />.
/// </summary>
/// <remarks>
/// It is recommended to always include this attribute to allow the user to better understand the system which they are
/// using.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class DescriptionAttribute : Attribute, IVoiceCommandDescriptor
{
    public DescriptionAttribute(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);
        Description = tag;
    }

    /// <summary>
    /// User readable description.
    /// </summary>
    public string Description { get; }
}
