using System.Diagnostics;

using Willow.DeviceAutomation.InputDevices.Enums;
using Willow.DeviceAutomation.InputDevices.Exceptions;
using Willow.DeviceAutomation.InputDevices.Windows.Extensions;

namespace Willow.DeviceAutomation.InputDevices.Windows;

internal sealed partial class InputSimulator
{
    public IInputSimulator KeyDown(Key key)
    {
        var input = GetKeyInput(key, false);
        SendInputCore(input);
        return this;
    }

    public IInputSimulator KeyUp(Key key)
    {
        var input = GetKeyInput(key, true);
        SendInputCore(input);
        return this;
    }

    public IInputSimulator PressKey(Key key)
    {
        KeyDown(key);
        KeyUp(key);
        return this;
    }

    public IInputSimulator PressKey(params Key[] keys)
    {
        foreach (var key in keys)
        {
            KeyDown(key);
            Thread.Sleep(100);
        }

        foreach (var key in keys.Reverse())
        {
            KeyUp(key);
            Thread.Sleep(100);
        }

        return this;
    }

    public IInputSimulator Type(char character)
    {
        var input = GetInputAsUnicode(character);
        SendInputCore(input);
        return this;
    }

    public IInputSimulator Type(string input)
    {
        using var locker = _lock.Lock();
        CopyStringToClipboard(input);
        PressKey(Key.LeftCommandOrControl, Key.V);
        return this;
    }

    private static Input GetKeyInput(Key key, bool release)
    {
        var virtualKeyCode = key.GetVirtualKeyCode();
        Input input;
        if (virtualKeyCode == 0)
        {
            input = GetInputAsUnicode(key.GetAssociatedChar());
        }
        else if (IsTypicallyExtended(key))
        {
            input = GetInputAsVirtualKey(virtualKeyCode);
        }
        else
        {
            input = GetInputAsScanCode(key, virtualKeyCode);
        }

        if (release)
        {
            input.InputUnion.KeyboardInput.KeyEvents |= KeyEvents.KeyUp;
        }

        return input;
    }

    //MapVirtualKeyExW appears to be broken and does not actually return the extended key flag, there is a limit subset
    //of keys that are typically extended, going to try to send them as the virtualKeyCode instead of mapping to scan
    //another class of keys that tend to fail is modifer keys, so they also get special treatment
    private static bool IsTypicallyExtended(Key key)
    {
        return key switch
        {
            Key.DownArrow
                or Key.UpArrow
                or Key.LeftArrow
                or Key.RightArrow
                or Key.Insert
                or Key.Home
                or Key.PageDown
                or Key.PageUp
                or Key.End
                or Key.Delete
                or Key.LeftCommandOrControl
                or Key.RightCommandOrControl
                or Key.LeftShift
                or Key.RightShift
                or Key.LeftAltOrOption
                or Key.RightAltOrOption => true,
            _ => false
        };
    }

    private static Input GetInputAsVirtualKey(ushort key)
    {
        return Input.CreateKeyboardInput(new KeyboardInput() { VirtualKeyCode = key });
    }

    private static Input GetInputAsScanCode(Key key, ushort virtualKeyCode)
    {
        var scanCode = GetScanCode(key, virtualKeyCode);

        return Input.CreateKeyboardInput(new KeyboardInput()
        {
            ScanCode = scanCode,
            KeyEvents = KeyEvents.Scancode
                        | (IsExtended(scanCode) ? KeyEvents.ExtendedKey : 0)
        });

        //I'm leaving the function in but MapVirtualKeyExW is broken
        bool IsExtended(ushort scanCodeToCheck)
        {
            var upperByte = scanCodeToCheck >> 8;
            return upperByte is 0xE1 or 0xE0; //Defined by windows as the upper byte for extensions
        }
    }

    private static ushort GetScanCode(Key key, ushort virtualKeyCode)
    {
        const uint VirtualKeyToScanCodeExtendedMappingCode = 4;

        var keyboardHandle = GetKeyboardLayout(0);
        var scanCode = MapVirtualKeyExW(virtualKeyCode, VirtualKeyToScanCodeExtendedMappingCode, keyboardHandle);

        if (scanCode == 0)
        {
            throw new InvalidOsKeyException(key);
        }

        return (ushort)scanCode;
    }

    private static Input GetInputAsUnicode(char associatedChar)
    {
        return Input.CreateKeyboardInput(
            new KeyboardInput() { ScanCode = associatedChar, KeyEvents = KeyEvents.Unicode });
    }
}
