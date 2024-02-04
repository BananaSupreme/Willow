using Desktop.Robot;

using Willow.Helpers.Locking;

namespace Willow.DeviceAutomation.InputDevices;

internal sealed partial class InputSimulator : IInputSimulator
{
    private static readonly DisposableLock _lock = new();
    private readonly IRobot _robot;

    public InputSimulator(IRobot robot)
    {
        _robot = robot;
    }
}
