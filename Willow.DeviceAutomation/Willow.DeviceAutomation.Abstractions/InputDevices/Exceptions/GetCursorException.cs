using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Willow.DeviceAutomation.InputDevices.Exceptions;

public sealed class GetCursorException : Exception
{
    public GetCursorException() : base("Failed to get the cursor position, check inner error",
                                       new Win32Exception(Marshal.GetLastWin32Error()))
    {
    }
}
