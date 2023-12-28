using System.Diagnostics;
using System.Numerics;

using Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;
using Willow.Core.SpeechCommands.ScriptingInterface.Models;
using Willow.DeviceAutomation.InputDevices.Abstractions;

namespace Willow.BuiltInCommands.MouseCommands;

internal sealed class ScrollVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;
    
    private const int _defaultAmount = 10;
    private const string _up = "up";
    private const string _down = "down";
    private const string _direction = "direction";
    private const string _amount = "amount";

    public ScrollVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }
    
    public string InvocationPhrase => $"scroll [{_up}|{_down}]:{_direction} ?[#{_amount}]:hit";
    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var amount = context.Parameters.GetValueOrDefault(_amount)?.GetInt32() ?? _defaultAmount;
        var direction = context.Parameters.GetValueOrDefault(_direction)?.GetString() ??
                        throw new UnreachableException();
        var baseVector = GetFromDirection(direction);

        _inputSimulator.Scroll(amount * baseVector);
        
        return Task.CompletedTask;
    }
    
    private Vector2 GetFromDirection(string direction)
    {
        return direction switch
        {
            _up => new(0, -1),
            _down => new(0, 1),
            _ => throw new UnreachableException()
        };
    }
}