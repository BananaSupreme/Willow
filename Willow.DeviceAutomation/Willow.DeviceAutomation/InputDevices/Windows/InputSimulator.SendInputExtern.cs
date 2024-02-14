using System.Runtime.InteropServices;

namespace Willow.DeviceAutomation.InputDevices.Windows;

internal sealed partial class InputSimulator
{
    [LibraryImport("user32.dll")]
    private static partial nint GetKeyboardLayout(int idThread);

    [LibraryImport("user32.dll")]
    private static partial uint MapVirtualKeyExW(uint code, uint mapType, nint keyboardHandle);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial uint SendInput(uint nInputs,
                                          [MarshalAs(UnmanagedType.LPArray)] [In] Input[] pInputs,
                                          int cbSize);

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
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        public ushort VirtualKeyCode;
        public ushort ScanCode;
        public KeyEvents KeyEvents;
        public uint Time;
        public IntPtr ExtraInfo;
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
        HWheel = 0x1000, // >= Win Vista only
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
}
