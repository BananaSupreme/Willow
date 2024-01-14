using Willow.Speech.ScriptingInterface.Abstractions;

namespace Willow.Speech.ScriptingInterface.Attributes;

/// <summary>
/// Defines a user readable description of the <see cref="IVoiceCommand"/>.
/// </summary>
/// <remarks>
/// It is recommended to always include this attribute to allow the user to better understand the system which they are
/// using.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DescriptionAttribute : Attribute
{
    /// <summary>
    /// User readable description.
    /// </summary>
    public string Description { get; }

    public DescriptionAttribute(string tag)
    {
        Description = tag;
    }
}