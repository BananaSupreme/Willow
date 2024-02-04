using System.Diagnostics.CodeAnalysis;

namespace Willow.Environment.Enums;

/// <summary>
/// The Operating systems supported by object,
/// if an OS is not mentioned here it should still be considered that in the future support for it might
/// added, and so maintainers should take into consideration when adding for example windows or linux specific
/// commands.
/// <remarks>
/// If an operating system doesn't appear here it means no official support for it exists yet.
/// </remarks>
/// </summary>
[Flags]
[SuppressMessage("Design",
                 "CA1069:Enums values should not be duplicated")] // Only while the only supported os is windows
public enum SupportedOss
{
    None = 0,
    Windows = 1,
    All = 1
}

public static class SupportedOssExtensions
{
    public static bool HasFlagFast(this SupportedOss value, SupportedOss flag)
    {
        return (value & flag) != 0;
    }
}
