using Willow.DeviceAutomation.InputDevices.Enums;

namespace Willow.DeviceAutomation.InputDevices.Windows.Exceptions;

public sealed class InvalidVirtualKeyException : InvalidOperationException
{
    public InvalidVirtualKeyException(Key key) : base(
        $"{key} has no valid representation as a windows virtual key or scan code on this keyboard")
    {
    }
}
