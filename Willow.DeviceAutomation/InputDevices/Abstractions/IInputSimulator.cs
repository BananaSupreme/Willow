using System.Numerics;

using Willow.DeviceAutomation.InputDevices.Enums;

namespace Willow.DeviceAutomation.InputDevices.Abstractions;

public interface IInputSimulator
{
    IInputSimulator KeyDown(Key key);
    IInputSimulator KeyUp(Key key);
    IInputSimulator PressKey(Key key);
    IInputSimulator PressKey(params Key[] keys);
    IInputSimulator Type(string input);
    
    Vector2 CursorPosition { get; }
    IInputSimulator Click(MouseButton key = MouseButton.Left);
    IInputSimulator MouseButtonDown(MouseButton key = MouseButton.Left);
    IInputSimulator MouseButtonUp(MouseButton key = MouseButton.Left);
    IInputSimulator Scroll(Vector2 amount);
    IInputSimulator MoveCursorToOffset(Vector2 vector);
    IInputSimulator MoveCursorToAbsolute(Vector2 position);
    /*
     * We cannot implement those function since we cannot get the screen size at the moment
     * Vector2 CurrentMonitorSize { get; }
     * IInputSimulator MoveCursorToPercentage(Vector2 position);
     */
}