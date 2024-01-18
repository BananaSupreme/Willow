using System.Diagnostics;
using System.Runtime.InteropServices;

using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Models;

namespace Willow.Core.Environment.ActiveWindowDetectors;

internal sealed partial class WindowsActiveWindowDetector : IActiveWindowDetector
{
    public ActiveWindowInfo GetActiveWindow()
    {
        var windowHandler = GetForegroundWindow();
        _ = GetWindowThreadProcessId(windowHandler, out var pid);
        var process = Process.GetProcessById((int)pid);

        return new ActiveWindowInfo(process.ProcessName);
    }

    [LibraryImport("user32.dll")]
    private static partial nint GetForegroundWindow();

    [LibraryImport("user32.dll")]
    // ReSharper disable once IdentifierTypo
    private static partial uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);
}
