using Desktop.Robot.Extensions;

using Willow.DeviceAutomation.InputDevices.Abstractions;
using Willow.DeviceAutomation.InputDevices.Enums;
using Willow.DeviceAutomation.InputDevices.Extensions;

namespace Willow.DeviceAutomation.InputDevices;

internal sealed partial class InputSimulator
{
    public IInputSimulator KeyDown(Key key)
    {
        using var locker = _lock.Lock();
        _robot.KeyDown(key.ToRobot());
        return this;
    }

    public IInputSimulator KeyUp(Key key)
    {
        using var locker = _lock.Lock();
        _robot.KeyUp(key.ToRobot());
        return this;
    }

    public IInputSimulator PressKey(Key key)
    {
        using var locker = _lock.Lock();
        _robot.KeyPress(key.ToRobot());
        return this;
    }

    public IInputSimulator PressKey(params Key[] keys)
    {
        using var locker = _lock.Lock();
        _robot.CombineKeys(keys.Select(static x => x.ToRobot()).ToArray());
        return this;
    }

    public IInputSimulator Type(string input)
    {
        using var locker = _lock.Lock();
        _robot.Type(input);
        return this;
    }
}
