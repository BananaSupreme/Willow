using Willow.DeviceAutomation.InputDevices.Enums;
using Willow.DeviceAutomation.InputDevices.Windows.Exceptions;
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
        var input = GetInputFromChar(character);
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

    private static readonly Input[] _cachedArray = new Input[1];

    private static void SendInputCore(Input input)
    {
        _cachedArray[0] = input;
        var sent = SendInput((uint)_cachedArray.Length, _cachedArray, Input.Size);
        if (sent != 1)
        {
            throw new KeyboardPressException();
        }
    }

    private static Input GetKeyInput(Key key, bool release)
    {
        var virtualKeyCode = key.GetVirtualKeyCode();
        var input = virtualKeyCode != 0
                        ? GetInputFromVirtualKey(key, virtualKeyCode)
                        : GetInputFromChar(key.GetAssociatedChar());
        if (release)
        {
            input.InputUnion.KeyboardInput.KeyEvents |= KeyEvents.KeyUp;
        }

        return input;
    }

    private static Input GetInputFromVirtualKey(Key key, ushort virtualKeyCode)
    {
        var scanCode = GetScanCode(key, virtualKeyCode);

        return Input.CreateKeyboardInput(new KeyboardInput()
        {
            ScanCode = scanCode,
            KeyEvents = KeyEvents.Scancode
                        | (IsExtended(scanCode) ? KeyEvents.ExtendedKey : 0)
        });

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
        var scanCode = (ushort)MapVirtualKeyExW(virtualKeyCode, VirtualKeyToScanCodeExtendedMappingCode, keyboardHandle);

        if (scanCode == 0)
        {
            throw new InvalidVirtualKeyException(key);
        }

        return scanCode;
    }

    private static Input GetInputFromChar(char associatedChar)
    {
        return Input.CreateKeyboardInput(new KeyboardInput() { ScanCode = associatedChar, KeyEvents = KeyEvents.Unicode });
    }
}
