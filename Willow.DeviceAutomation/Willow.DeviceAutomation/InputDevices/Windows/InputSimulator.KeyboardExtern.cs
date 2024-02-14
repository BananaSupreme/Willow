using System.Runtime.InteropServices;

// ReSharper disable UnusedMethodReturnValue.Local
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// Marshalling is a broken naming world.

namespace Willow.DeviceAutomation.InputDevices.Windows;

internal sealed partial class InputSimulator
{
    private const int KeyboardInput = 1;

    [LibraryImport("user32.dll")]
    private static partial nint GetKeyboardLayout(int idThread);

    [LibraryImport("user32.dll")]
    private static partial uint MapVirtualKeyExW(uint uCode, uint uMapType, nint dwhkl);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial uint SendInput(uint nInputs,
                                          [MarshalAs(UnmanagedType.LPArray)] [In] INPUT[] pInputs,
                                          int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public int type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public nuint dwExtraInfo;
    }
}
