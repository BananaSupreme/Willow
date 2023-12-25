using Desktop.Robot;

using Willow.Core.Helpers;
using Willow.DeviceAutomation.InputDevices.Abstractions;

namespace Willow.DeviceAutomation.InputDevices;

internal partial class InputSimulator : IInputSimulator
{
    private readonly IRobot _robot;
    private static readonly DisposableLock _lock = new();

    public InputSimulator(IRobot robot)
    {
        _robot = robot;
    }
}