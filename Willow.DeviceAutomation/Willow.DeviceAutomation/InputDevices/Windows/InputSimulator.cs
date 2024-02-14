using System.Numerics;

using Willow.Helpers.Locking;

namespace Willow.DeviceAutomation.InputDevices.Windows;

internal sealed partial class InputSimulator : IInputSimulator
{
    private static readonly DisposableLock _lock = new();

    public Vector2 CurrentMonitorSize => new();
}
