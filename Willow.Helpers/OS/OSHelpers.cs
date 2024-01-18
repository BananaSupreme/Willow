using System.Diagnostics;

namespace Willow.Helpers.OS;

public static class OsHelpers
{
    /// <summary>
    /// A wrapper that returns <typeparamref name="T" /> based on the current OS
    /// </summary>
    /// <remarks>
    /// Implementors creating extensions that are OS dependent can use this helper if they want to support all supported
    /// operating system.
    /// <b><i>Be aware that adding a supported OS is a breaking change here.</i></b>
    /// </remarks>
    /// <param name="windowsFunc">The function to run when the current OS is windows.</param>
    /// <typeparam name="T">The returned type</typeparam>
    public static T MatchOs<T>(Func<T> windowsFunc)
    {
        if (OperatingSystem.IsWindows())
        {
            return windowsFunc();
        }

        throw new UnreachableException();
    }

    /// <summary>
    /// A wrapper that executes an action based on the current OS
    /// </summary>
    /// <remarks>
    /// Implementors creating extensions that are OS dependent can use this helper if they want to support all supported
    /// operating system.
    /// <b><i>Be aware that adding a supported OS is a breaking change here.</i></b>
    /// </remarks>
    /// <param name="windowsFunc">The function to run when the current OS is windows.</param>
    public static void MatchOs(Action windowsFunc)
    {
        if (OperatingSystem.IsWindows())
        {
            windowsFunc();
        }
    }
}
