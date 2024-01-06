using System.Diagnostics;
using System.Numerics;

using Willow.DeviceAutomation.InputDevices.Abstractions;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.MouseCommands.Scroll;

internal sealed class ScrollVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    public ScrollVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public string InvocationPhrase => $"scroll [up|down]:direction ?[#amount]:hit";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var amount = context.Parameters.GetValueOrDefault("amount")?.GetInt32() ?? 10;
        var direction = context.Parameters.GetValueOrDefault("direction")?.GetString() ??
                        throw new UnreachableException();
        var baseVector = GetFromDirection(direction);

        _inputSimulator.Scroll(amount * baseVector);

        return Task.CompletedTask;
    }

    private Vector2 GetFromDirection(string direction)
    {
        return direction switch
        {
            "up" => new(0, -1),
            "down" => new(0, 1),
            _ => throw new UnreachableException()
        };
    }
}