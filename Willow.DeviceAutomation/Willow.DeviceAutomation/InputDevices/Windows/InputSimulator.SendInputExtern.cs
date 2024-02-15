using System.Runtime.InteropServices;

using Willow.DeviceAutomation.InputDevices.Exceptions;

namespace Willow.DeviceAutomation.InputDevices.Windows;

internal sealed partial class InputSimulator
{
    [LibraryImport("user32.dll")]
    private static partial nint GetKeyboardLayout(int idThread);

    [LibraryImport("user32.dll")]
    private static partial uint MapVirtualKeyExW(uint code, uint mapType, nint keyboardHandle);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial uint SendInput(uint numberOfInputs,
                                          [MarshalAs(UnmanagedType.LPArray)] [In] Input[] inputs,
                                          int sizeOfInput);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(ref Win32Point win32Point);

    private static readonly Input[] _cachedInputArray = new Input[1];

    private static void SendInputCore(Input input)
    {
        _cachedInputArray[0] = input;
        var sent = SendInput((uint)_cachedInputArray.Length, _cachedInputArray, Input.Size);
        if (sent != 1)
        {
            throw new SendInputException();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Input
    {
        public InputType InputType;
        public InputUnion InputUnion;

        public static int Size => Marshal.SizeOf<Input>();

        public static Input CreateMouseInput(MouseInput mouseInput)
        {
            return new Input
            {
                InputType = InputType.MouseInput, InputUnion = new InputUnion { MouseInput = mouseInput }
            };
        }

        public static Input CreateKeyboardInput(KeyboardInput keyboardInput)
        {
            return new Input
            {
                InputType = InputType.KeyboardInput, InputUnion = new InputUnion { KeyboardInput = keyboardInput }
            };
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)] public MouseInput MouseInput;

        [FieldOffset(0)] public KeyboardInput KeyboardInput;

        [FieldOffset(0)] public HardwareInput HardwareInput;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        public int DeltaX;
        public int DeltaY;
        public uint MouseData;
        public MouseEvents MouseEvents;
        public uint Time;
        public nint ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        public ushort VirtualKeyCode;
        public ushort ScanCode;
        public KeyEvents KeyEvents;
        public uint Time;
        public nint ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HardwareInput
    {
        public uint Message;
        public ushort ParamL;
        public ushort ParamH;
    }

    public enum InputType : uint
    {
        MouseInput = 0,
        KeyboardInput = 1
    }

    [Flags]
    public enum MouseEvents : uint
    {
        Move = 0x0001,
        LeftDown = 0x0002,
        LeftUp = 0x0004,
        RightDown = 0x0008,
        RightUp = 0x0010,
        MiddleDown = 0x0020,
        MiddleUp = 0x0040,
        XDown = 0x0080,
        XUp = 0x0100,
        Wheel = 0x0800,
        HorizontalWheel = 0x1000, // >= Win Vista only
        MoveNoCoalesce = 0x2000,
        VirtualDesk = 0x4000,
        Absolute = 0x8000
    }

    [Flags]
    public enum KeyEvents : uint
    {
        ExtendedKey = 0x0001,
        KeyUp = 0x0002,
        Unicode = 0x0004,
        Scancode = 0x0008
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Win32Point
    {
        public int X;
        public int Y;
    }
}
