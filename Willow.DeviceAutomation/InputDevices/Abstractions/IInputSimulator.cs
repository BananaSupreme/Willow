using System.Numerics;

using Willow.DeviceAutomation.InputDevices.Enums;

namespace Willow.DeviceAutomation.InputDevices.Abstractions;

/// <summary>
/// GUI automation tools to use for controlling the underlying system.
/// </summary>
public interface IInputSimulator
{
    /// <summary>
    /// Gets the current cursor position on the screen.
    /// </summary>
    Vector2 CursorPosition { get; }

    /// <summary>
    /// Triggers the key to be held down, the key is not released automatically.
    /// </summary>
    /// <param name="key">The key to hold down.</param>
    IInputSimulator KeyDown(Key key);

    /// <summary>
    /// Releases a key that is held down.
    /// </summary>
    /// <param name="key">The key to release.</param>
    IInputSimulator KeyUp(Key key);

    /// <summary>
    /// Taps a key.
    /// </summary>
    /// <param name="key">The key to press.</param>
    IInputSimulator PressKey(Key key);

    /// <summary>
    /// Taps multiple keys together.
    /// </summary>
    /// <example>
    /// <c>PressKey(Key.LeftControl, Key.LeftShift, Key.C);</c>
    /// </example>
    /// <param name="keys">The keys to press together.</param>
    IInputSimulator PressKey(params Key[] keys);

    /// <summary>
    /// Types a string.
    /// </summary>
    /// <remarks>
    /// Currently implemented by tapping all the relevant keys in succession.
    /// </remarks>
    /// <param name="input">The input string to type.</param>
    IInputSimulator Type(string input);

    /// <summary>
    /// Clicks the mouse in its current position.
    /// </summary>
    /// <param name="button">The button to click.</param>
    /// <returns></returns>
    IInputSimulator Click(MouseButton button = MouseButton.Left);

    /// <summary>
    /// Sets a mouse button down.
    /// </summary>
    /// <param name="button">The button to hold down.</param>
    IInputSimulator MouseButtonDown(MouseButton button = MouseButton.Left);

    /// <summary>
    /// releases a mouse button back up.
    /// </summary>
    /// <param name="button">The button to release.</param>
    IInputSimulator MouseButtonUp(MouseButton button = MouseButton.Left);

    /// <summary>
    /// Triggers a scroll motion.
    /// </summary>
    /// <param name="amount">The amount to scroll.</param>
    IInputSimulator Scroll(Vector2 amount);

    /// <summary>
    /// Moves the mouse by an offset.
    /// </summary>
    /// <param name="vector">The offset to move the mouse by in pixels.</param>
    IInputSimulator MoveCursorToOffset(Vector2 vector);

    /// <summary>
    /// Move mouse to an absolute position.
    /// </summary>
    /// <param name="position">The new mouse position in pixels on the screen.</param>
    /// <returns></returns>
    IInputSimulator MoveCursorToAbsolute(Vector2 position);
    /*
     * We cannot implement those function since we cannot get the screen size at the moment
     * Vector2 CurrentMonitorSize { get; } - something to avoid solar lint
     * IInputSimulator MoveCursorToPercentage(Vector2 position); - something to avoid solar lint
     */
}
