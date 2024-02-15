using System.Numerics;

using Willow.DeviceAutomation.InputDevices.Enums;
using Willow.DeviceAutomation.InputDevices.Exceptions;

namespace Willow.DeviceAutomation.InputDevices.Windows;

internal sealed partial class InputSimulator
{
    public Vector2 CursorPosition => GetCursorPosition();

    private static Vector2 GetCursorPosition()
    {
        var point = new Win32Point();
        var isSuccessful = GetCursorPos(ref point);
        if (!isSuccessful)
        {
            throw new SendInputException();
        }

        return new Vector2(point.X, point.Y);
    }

    public IInputSimulator Click(MouseButton button = MouseButton.Left)
    {
        switch (button)
        {
            case MouseButton.Left or MouseButton.Right:
                MouseButtonDown(button);
                MouseButtonUp(button);
                break;
            case MouseButton.Middle:
                var input = Input.CreateMouseInput(
                    new MouseInput() { MouseEvents = MouseEvents.Wheel, MouseData = 120u });
                SendInputCore(input);
                break;
            default:
                throw new InvalidOperationException("Cannot click none button");
        }

        return this;
    }

    public IInputSimulator MouseButtonDown(MouseButton button = MouseButton.Left)
    {
        var mouseFlag = button switch
        {
            MouseButton.Left => MouseEvents.LeftDown,
            MouseButton.Right => MouseEvents.RightDown,
            MouseButton.Middle => MouseEvents.MiddleDown,
            MouseButton.None => throw new InvalidOperationException("Cannot press no button")
        };

        var input = Input.CreateMouseInput(new MouseInput() { MouseEvents = mouseFlag });

        SendInputCore(input);
        return this;
    }

    public IInputSimulator MouseButtonUp(MouseButton button = MouseButton.Left)
    {
        var mouseFlag = button switch
        {
            MouseButton.Left => MouseEvents.LeftUp,
            MouseButton.Right => MouseEvents.RightUp,
            MouseButton.Middle => MouseEvents.MiddleUp,
            MouseButton.None => throw new InvalidOperationException("Cannot press no button")
        };

        var input = Input.CreateMouseInput(new MouseInput() { MouseEvents = mouseFlag });

        SendInputCore(input);
        return this;
    }

    public IInputSimulator Scroll(Vector2 amount)
    {
        //At 120 it is considered a click.
        if (amount.X > 0)
        {
            var inputVertical = Input.CreateMouseInput(new MouseInput()
            {
                MouseEvents = MouseEvents.Wheel,
                MouseData = (uint)Math.Min(amount.X, 119)
            });
            SendInputCore(inputVertical);
        }

        if (amount.Y > 0)
        {
            var inputHorizontal = Input.CreateMouseInput(new MouseInput()
            {
                MouseEvents = MouseEvents.HorizontalWheel,
                MouseData = (uint)Math.Min(amount.Y, 119)
            });
            SendInputCore(inputHorizontal);
        }

        return this;
    }

    public IInputSimulator MoveCursorToOffset(Vector2 vector)
    {
        var input = Input.CreateMouseInput(new MouseInput()
        {
            MouseEvents = MouseEvents.Move,
            DeltaY = (int)vector.Y,
            DeltaX = (int)vector.X
        });
        SendInputCore(input);
        return this;
    }

    public IInputSimulator MoveCursorToAbsolute(Vector2 position)
    {
        var input = Input.CreateMouseInput(new MouseInput()
        {
            MouseEvents = MouseEvents.Move | MouseEvents.Absolute,
            DeltaY = (int)position.Y,
            DeltaX = (int)position.X
        });
        SendInputCore(input);
        return this;
    }
}
