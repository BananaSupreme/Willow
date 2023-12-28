using System.Diagnostics;
using System.Numerics;

using Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;
using Willow.Core.SpeechCommands.ScriptingInterface.Models;
using Willow.DeviceAutomation.InputDevices.Abstractions;

namespace Willow.BuiltInCommands.MouseCommands;

internal sealed class MoveMouseByOffsetVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    private const int _defaultAmount = 50;
    private const string _left = "left";
    private const string _right = "right";
    private const string _up = "up";
    private const string _down = "down";
    private const string _direction = "direction";
    private const string _amount = "amount";
    
    public MoveMouseByOffsetVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }
    
    public string InvocationPhrase => $"move mouse [{_left}|{_right}|{_up}|{_down}]:{_direction} ?[#{_amount}]:_";
    
    
    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var direction = context.Parameters.GetValueOrDefault(_direction)?.GetString()!;
        var baseVector = GetFromDirection(direction);

        var amount = context.Parameters.GetValueOrDefault(_amount)?.GetInt32() ?? _defaultAmount;

        _inputSimulator.MoveCursorToOffset(baseVector * amount);
        return Task.CompletedTask;
    }

    private Vector2 GetFromDirection(string direction)
    {
        return direction switch
        {
            _right => new(1, 0),
            _left => new(-1, 0),
            _up => new(0, -1),
            _down => new(0, 1),
            _ => throw new UnreachableException()
        };
    }
}