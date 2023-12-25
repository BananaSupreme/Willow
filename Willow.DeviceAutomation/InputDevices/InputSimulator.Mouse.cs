using System.Numerics;

using Willow.DeviceAutomation.InputDevices.Abstractions;
using Willow.DeviceAutomation.InputDevices.Enums;
using Willow.DeviceAutomation.InputDevices.Extensions;

namespace Willow.DeviceAutomation.InputDevices;

internal partial class InputSimulator
{
    public Vector2 CursorPosition => _robot.GetMousePosition().FromPoint();

    public IInputSimulator Click(MouseButton key = MouseButton.Left)
    {
        using var locker = _lock.Lock();
        _robot.Click(key.ToRobotClick());
        return this;
    }

    public IInputSimulator MouseButtonDown(MouseButton key = MouseButton.Left)
    {
        using var locker = _lock.Lock();
        _robot.MouseDown(key.ToRobotClick());
        return this;
    }

    public IInputSimulator MouseButtonUp(MouseButton key = MouseButton.Left)
    {
        using var locker = _lock.Lock();
        _robot.MouseUp(key.ToRobotClick());
        return this;
    }

    //Horizontal scroll not implemented yet
    public IInputSimulator Scroll(Vector2 amount)
    {
        using var locker = _lock.Lock();
        _robot.MouseScroll((int)amount.Y);
        return this;
    }

    public IInputSimulator MoveCursorToOffset(Vector2 vector)
    {
        var newPosition = vector + CursorPosition;
        MoveCursorToAbsolute(newPosition);
        return this;
    }

    public IInputSimulator MoveCursorToAbsolute(Vector2 position)
    {
        using var locker = _lock.Lock();
        _robot.MouseMove(position.FromVector2());
        return this;
    }
}