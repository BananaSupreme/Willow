using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Willow.DeviceAutomation.InputDevices.Windows.Exceptions;

public sealed class ClipboardException : Exception
{
    public ClipboardException() : base("Clipboard failed to open, windows exception set in the inner error",
                                       new Win32Exception(Marshal.GetLastWin32Error()))
    {
    }
}
