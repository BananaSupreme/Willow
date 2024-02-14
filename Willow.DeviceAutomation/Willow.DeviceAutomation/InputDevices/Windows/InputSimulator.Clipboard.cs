using System.Runtime.InteropServices;

using Willow.DeviceAutomation.InputDevices.Windows.Exceptions;

namespace Willow.DeviceAutomation.InputDevices.Windows;

internal sealed partial class InputSimulator
{
    private static void CopyStringToClipboard(string input)
    {
        TryOpenClipboard();

        try
        {
            TryEmptyClipboard();
            var allocationPointer = TryAllocateGlobalMemory(input);
            try
            {
                var lockPointer = TryLockMemory(allocationPointer);
                try
                {
                    TrySetValueAtClipboard(input, lockPointer, allocationPointer);
                    allocationPointer = nint.Zero;
                }
                finally
                {
                    GlobalUnlock(lockPointer);
                }
            }
            catch
            {
                GlobalFree(allocationPointer);
                throw;
            }
        }
        finally
        {
            CloseClipboard();
        }
    }

    private static void TrySetValueAtClipboard(string input, nint lockPointer, nint allocationPointer)
    {
        const uint UnicodeTextFlag = 13;
        Marshal.Copy(input.ToCharArray(), 0, lockPointer, input.Length);
        Marshal.WriteInt16(lockPointer, input.Length * 2, 0);
        if (SetClipboardData(UnicodeTextFlag, allocationPointer) == nint.Zero)
        {
            throw new ClipboardException();
        }
    }

    private static nint TryLockMemory(nint allocationPointer)
    {
        var returnedPointer = GlobalLock(allocationPointer);
        if (returnedPointer == nint.Zero)
        {
            throw new ClipboardException();
        }

        return returnedPointer;
    }

    private static nint TryAllocateGlobalMemory(string input)
    {
        const uint MovableMemoryFlag = 2;

        var allocationPointer = GlobalAlloc(MovableMemoryFlag, (nuint)((input.Length + 1) * 2));
        if (allocationPointer == nint.Zero)
        {
            throw new ClipboardException();
        }

        return allocationPointer;
    }

    private static void TryOpenClipboard()
    {
        if (!OpenClipboard(nint.Zero))
        {
            throw new ClipboardException();
        }
    }

    private static void TryEmptyClipboard()
    {
        if (!EmptyClipboard())
        {
            throw new ClipboardException();
        }
    }
}
