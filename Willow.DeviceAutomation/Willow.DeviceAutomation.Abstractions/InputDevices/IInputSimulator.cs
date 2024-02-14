using System.Numerics;

using Willow.DeviceAutomation.InputDevices.Enums;

namespace Willow.DeviceAutomation.InputDevices;

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
    /// Gets the size of the current active monitor.
    /// </summary>
    Vector2 CurrentMonitorSize { get; }

    /// <summary>
    /// Triggers the key to be held down, the key is not released automatically.
    /// </summary>
    /// <remarks>
    /// Certain keys like <see cref="Key.Ampersand"/> don't have an actual key on the keyboard so they are called on
    /// windows using a call to the unicode representation which might not work as well when trying to use the key as a
    /// command.
    /// </remarks>
    /// <param name="key">The key to hold down.</param>
    IInputSimulator KeyDown(Key key);

    /// <summary>
    /// Releases a key that is held down.
    /// </summary>
    /// <remarks>
    /// Certain keys like <see cref="Key.Ampersand"/> don't have an actual key on the keyboard so they are called on
    /// windows using a call to the unicode representation which might not work as well when trying to use the key as a
    /// command.
    /// </remarks>
    /// <param name="key">The key to release.</param>
    IInputSimulator KeyUp(Key key);

    /// <summary>
    /// Taps a key.
    /// </summary>
    /// <remarks>
    /// Certain keys like <see cref="Key.Ampersand"/> don't have an actual key on the keyboard so they are called on
    /// windows using a call to the unicode representation which might not work as well when trying to use the key as a
    /// command.
    /// </remarks>
    /// <param name="key">The key to press.</param>
    IInputSimulator PressKey(Key key);

    /// <summary>
    /// Taps multiple keys together.
    /// </summary>
    /// <example>
    /// <c>PressKey(Key.LeftControl, Key.LeftShift, Key.C);</c>
    /// </example>
    /// <remarks>
    /// Certain keys like <see cref="Key.Ampersand"/> don't have an actual key on the keyboard so they are called on
    /// windows using a call to the unicode representation which might not work as well when trying to use the key as a
    /// command.
    /// </remarks>
    /// <param name="keys">The keys to press together.</param>
    IInputSimulator PressKey(params Key[] keys);

    /// <summary>
    /// Taps a character.
    /// </summary>
    /// <remarks>
    /// In windows implemented by sending character as a unicode value
    /// </remarks>
    /// <param name="character">The character character to type.</param>
    IInputSimulator Type(char character);

    /// <summary>
    /// Types a string.
    /// </summary>
    /// <remarks>
    /// In windows implemented by copying to clipboard and pasting with Ctrl/Command + V
    /// </remarks>
    /// <param name="input">The character string to type.</param>
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
}
