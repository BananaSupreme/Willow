using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Willow.DeviceAutomation.InputDevices.Windows.Exceptions;

public sealed class KeyboardPressException : Exception
{
    public KeyboardPressException() : base("Failed to send keyboard event, check inner error",
                                           new Win32Exception(Marshal.GetLastWin32Error()))
    {
    }
}
