using Willow.DeviceAutomation.InputDevices.Enums;

namespace Willow.DeviceAutomation.InputDevices.Exceptions;

public sealed class InvalidOsKeyException : InvalidOperationException
{
    public InvalidOsKeyException(Key key) : base(
        $"{key} has no valid representation as a key in this operating system or scan code on this keyboard")
    {
    }
}
