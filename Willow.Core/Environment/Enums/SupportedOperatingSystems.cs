namespace Willow.Core.Environment.Enums;

[Flags]
public enum SupportedOperatingSystems
{
    None = 0,
    Windows = 1,
    All = 1
}

public static class SupportedOperatingSystemsExtensions
{
    public static bool HasFlagFast(this SupportedOperatingSystems value, SupportedOperatingSystems flag)
    {
        return (value & flag) != 0;
    }
}