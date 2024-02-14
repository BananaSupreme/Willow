using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;

using Willow.DeviceAutomation.InputDevices.Enums;
using Willow.DeviceAutomation.InputDevices.Windows.Exceptions;
using Willow.DeviceAutomation.InputDevices.Windows.Extensions;

namespace Willow.DeviceAutomation.InputDevices.Windows;

internal sealed partial class InputSimulator
{
    public IInputSimulator KeyDown(Key key)
    {
        using var locker = _lock.Lock();
        var input = GetKeyInput(key, false);
        SendInputCore(input);
        return this;
    }

    public IInputSimulator KeyUp(Key key)
    {
        using var locker = _lock.Lock();
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
        using var locker = _lock.Lock();
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
        using var locker = _lock.Lock();
        var input = GetInputFromChar(character);
        SendInputCore(input);
        return this;
    }

    public IInputSimulator Type(string input)
    {
        using var locker = _lock.Lock();
        CopyStringToClipboard(input);
        PressKey(Key.LeftAltOrOption, Key.V);
        return this;
    }

    private static readonly INPUT[] _cachedArray = new INPUT[1];

    private static void SendInputCore(INPUT input)
    {
        _cachedArray[0] = input;
        var sent = SendInput(1, _cachedArray, Marshal.SizeOf<INPUT>());
        if (sent != 1)
        {
            throw new KeyboardPressException();
        }
    }

    private static INPUT GetKeyInput(Key key, bool release)
    {
        const uint ReleaseKeyKeyboardFlag = 2;
        var virtualKeyCode = key.GetVirtualKeyCode();
        var input = virtualKeyCode != 0
                        ? GetInputFromVirtualKey(key, virtualKeyCode)
                        : GetInputFromChar(key.GetAssociatedChar());
        if (release)
        {
            input.U.ki.dwFlags |= ReleaseKeyKeyboardFlag;
        }

        return input;
    }

    private static INPUT GetInputFromVirtualKey(Key key, ushort virtualKeyCode)
    {
        const uint UseScanCodeFlag = 8;
        const uint ExtendedKeyFlag = 1;

        var scanCode = GetScanCode(key, virtualKeyCode);

        return new INPUT()
        {
            type = KeyboardInput,
            U = new InputUnion()
            {
                ki = new KEYBDINPUT()
                {
                    wScan = scanCode,
                    dwFlags = UseScanCodeFlag | (IsExtended(scanCode) ? ExtendedKeyFlag : 0)
                }
            }
        };

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

    private static INPUT GetInputFromChar(char associatedChar)
    {
        const uint UnicodeFlag = 4;

        return new INPUT()
        {
            type = KeyboardInput,
            U = new InputUnion() { ki = new KEYBDINPUT() { wScan = associatedChar, dwFlags = UnicodeFlag } }
        };
    }
}
