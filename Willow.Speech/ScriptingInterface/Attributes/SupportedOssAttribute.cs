using Willow.Core.Environment.Enums;
using Willow.Speech.ScriptingInterface.Abstractions;

namespace Willow.Speech.ScriptingInterface.Attributes;

/// <summary>
/// Flags the OSs that this command supports, any commands not supported by the user current OS will not be included.
/// </summary>
/// <remarks>
/// If this attribute is not defined the system assumes <c>SupportedOss.Any</c>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SupportedOssAttribute : Attribute, IVoiceCommandDescriptor
{
    public SupportedOssAttribute(SupportedOss supportedOss)
    {
        SupportedOss = supportedOss;
    }

    public SupportedOss SupportedOss { get; }
}
