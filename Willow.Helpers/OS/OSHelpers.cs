using System.Diagnostics;

namespace Willow.Helpers.OS;

internal static class OsHelpers
{
    public static T MatchOs<T>(Func<T> windowsFunc, Func<T> macFunc, Func<T> linuxFunc)
    {
        if (OperatingSystem.IsWindows())
        {
            return windowsFunc();
        }

        else if (OperatingSystem.IsMacOS())
        {
            return macFunc();
        }

        else if (OperatingSystem.IsLinux())
        {
            return linuxFunc();
        }

        throw new UnreachableException();
    }

    public static void MatchOs(Action windowsFunc, Action macFunc, Action linuxFunc)
    {
        if (OperatingSystem.IsWindows())
        {
            windowsFunc();
        }

        else if (OperatingSystem.IsMacOS())
        {
            macFunc();
        }

        else if (OperatingSystem.IsLinux())
        {
            linuxFunc();
        }
    }
}