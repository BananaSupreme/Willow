using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Willow.DeviceAutomation.InputDevices.Exceptions;

public sealed class SendInputException : Exception
{
    public SendInputException() : base("Failed to send keyboard event, check inner error",
                                       new Win32Exception(Marshal.GetLastWin32Error()))
    {
    }
}
