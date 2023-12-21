using Desktop.Robot.Clicks;

using Willow.DeviceAutomation.InputDevices.Enums;

namespace Willow.DeviceAutomation.InputDevices.Extensions;

internal static class MouseButtonExtensions
{
    public static IClick ToRobotClick(this MouseButton mouseButton)
    {
        return mouseButton switch
        {
            MouseButton.Left => Mouse.LeftButton(),
            MouseButton.Middle => Mouse.MiddleButton(),
            MouseButton.Right => Mouse.RightButton(),
            MouseButton.None => throw new InvalidOperationException("Invalid value none for mouse click")
        };
    }
}