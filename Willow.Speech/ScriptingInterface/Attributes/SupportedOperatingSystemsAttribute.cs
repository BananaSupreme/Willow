using Willow.Core.Environment.Enums;

namespace Willow.Speech.ScriptingInterface.Attributes;
[AttributeUsage(AttributeTargets.Class)]
public sealed class SupportedOperatingSystemsAttribute : Attribute
{
    public SupportedOperatingSystems SupportedOperatingSystems { get; }

    public SupportedOperatingSystemsAttribute(SupportedOperatingSystems supportedOperatingSystems)
    {
        SupportedOperatingSystems = supportedOperatingSystems;
    }
}