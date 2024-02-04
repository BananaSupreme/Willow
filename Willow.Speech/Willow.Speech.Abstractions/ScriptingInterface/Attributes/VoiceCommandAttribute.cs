using Willow.Environment.Models;

namespace Willow.Speech.ScriptingInterface.Attributes;

/// <summary>
/// Defines a group of tags that must all be active for the command to activate. That is that all the tags defined in
/// one attribute will be grouped into a singular <see cref="TagRequirement" />.<br/>
/// When multiple groups of tags are relevant, add multiple versions of this attribute.
/// </summary>
/// <remarks>
/// When no attribute exists it is assumed a singular attribute with no tags defined. That is it is relevant in all
/// environments.
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class VoiceCommandAttribute : Attribute, IVoiceCommandDescriptor
{
    public string InvocationPhrase { get; }

    public string[] RequiredMethods { get; set; } = [];

    public VoiceCommandAttribute(string invocationPhrase)
    {
        InvocationPhrase = invocationPhrase;
    }
}