using System.Diagnostics;

namespace Willow.Helpers.OS;

internal static class OsHelpers
{
    public static T MatchOs<T>(Func<T> windowsFunc)
    {
        if (OperatingSystem.IsWindows())
        {
            return windowsFunc();
        }

        throw new UnreachableException();
    }

    public static void MatchOs(Action windowsFunc)
    {
        if (OperatingSystem.IsWindows())
        {
            windowsFunc();
        }
    }
}