using System.Diagnostics;
using System.Runtime.InteropServices;

using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Models;

namespace Willow.Core.Environment.ActiveWindowDetectors;

internal sealed class WindowsActiveWindowDetector : IActiveWindowDetector
{
    public ActiveWindowInfo GetActiveWindow()
    {
        var windowHandler = GetForegroundWindow();
        GetWindowThreadProcessId(windowHandler, out var pid);
        var process = Process.GetProcessById((int)pid);

        return new(process.ProcessName);
    }
    
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
}