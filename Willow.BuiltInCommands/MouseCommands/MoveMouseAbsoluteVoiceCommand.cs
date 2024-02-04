using System.Numerics;

using Willow.DeviceAutomation.InputDevices;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.MouseCommands;

internal sealed class MoveMouseAbsoluteVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    public MoveMouseAbsoluteVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public string InvocationPhrase => "move mouse #horizontal #vertical";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var horizontal = context.Parameters["horizontal"].GetInt32();
        var vertical = context.Parameters["vertical"].GetInt32();

        _inputSimulator.MoveCursorToAbsolute(new Vector2(horizontal, vertical));
        return Task.CompletedTask;
    }
}
