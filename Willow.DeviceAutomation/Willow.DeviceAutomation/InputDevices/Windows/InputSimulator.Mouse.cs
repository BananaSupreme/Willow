using System.Numerics;

using Willow.DeviceAutomation.InputDevices.Enums;

namespace Willow.DeviceAutomation.InputDevices.Windows;

internal sealed partial class InputSimulator
{
    public Vector2 CursorPosition => new();

    public IInputSimulator Click(MouseButton button = MouseButton.Left)
    {
        using var locker = _lock.Lock();
        return this;
    }

    public IInputSimulator MouseButtonDown(MouseButton button = MouseButton.Left)
    {
        using var locker = _lock.Lock();
        return this;
    }

    public IInputSimulator MouseButtonUp(MouseButton button = MouseButton.Left)
    {
        using var locker = _lock.Lock();
        return this;
    }

    //Horizontal scroll not implemented yet
    public IInputSimulator Scroll(Vector2 amount)
    {
        using var locker = _lock.Lock();
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
        return this;
    }
}
