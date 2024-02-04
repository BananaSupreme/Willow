using System.Diagnostics;
using System.Numerics;

using Willow.DeviceAutomation.InputDevices;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.MouseCommands;

internal sealed class MoveMouseByOffsetVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    public MoveMouseByOffsetVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public string InvocationPhrase => "move mouse [left|right|up|down]:direction ?[#amount]:_";
    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var direction = context.Parameters.GetValueOrDefault("direction")?.GetString()!;
        var baseVector = GetFromDirection(direction);

        var amount = context.Parameters.GetValueOrDefault("amount")?.GetInt32() ?? 50;

        _inputSimulator.MoveCursorToOffset(baseVector * amount);
        return Task.CompletedTask;
    }

    private static Vector2 GetFromDirection(string direction)
    {
        return direction switch
        {
            "right" => new Vector2(1, 0),
            "left" => new Vector2(-1, 0),
            "up" => new Vector2(0, -1),
            "down" => new Vector2(0, 1),
            _ => throw new UnreachableException()
        };
    }
}
