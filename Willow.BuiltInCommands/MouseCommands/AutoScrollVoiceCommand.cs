using System.Diagnostics;
using System.Numerics;

using Willow.Core.Helpers;
using Willow.DeviceAutomation.InputDevices.Abstractions;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.MouseCommands;

internal sealed class AutoScrollVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    private static CancellationTokenSource? _cts;
    private static readonly DisposableLock _lock = new();

    private const string _start = "start";
    private const string _stop = "stop";
    private const string _action = "action";
    private const string _up = "up";
    private const string _down = "down";
    private const string _direction = "direction";

    public AutoScrollVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public string InvocationPhrase => $"scroll auto [{_start}|{_stop}]:{_action} ?[[{_up}|{_down}]:{_direction}]:_";

    public async Task ExecuteAsync(VoiceCommandContext context)
    {
        var action = context.Parameters.GetValueOrDefault(_action)?.GetString() ?? throw new UnreachableException();
        var direction = context.Parameters.GetValueOrDefault(_direction)?.GetString() ?? _down;

        switch (action)
        {
            case _start:
                _ = Start(direction, _inputSimulator);
                break;

            case _stop:
                await Stop();
                break;
        }
    }

    private static async Task Start(string direction, IInputSimulator inputSimulator)
    {
        using var unlocker = await _lock.LockAsync();
        
        if (_cts is not null)
        {
            return;
        }
        _cts = new();

        try
        {
            while (!_cts.IsCancellationRequested)
            {
                inputSimulator.Scroll(GetFromDirection(direction));
                await Task.Delay(100);
            }
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
        }
    }

    private static async Task Stop()
    {
        if (_cts is not null)
        {
            await _cts.CancelAsync();
        }
    }

    private static Vector2 GetFromDirection(string direction)
    {
        return direction switch
        {
            _up => new(0, -1),
            _down => new(0, 1),
            _ => throw new UnreachableException()
        };
    }
}