using Willow.DeviceAutomation.InputDevices.Abstractions;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.MouseCommands;

internal sealed class MoveMouseAbsoluteVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;
    
    public string InvocationPhrase => $"move mouse #horizontal #vertical";

    public MoveMouseAbsoluteVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }
    
    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var horizontal = context.Parameters["horizontal"].GetInt32();
        var vertical = context.Parameters["vertical"].GetInt32();

        _inputSimulator.MoveCursorToAbsolute(new(horizontal, vertical));
        return Task.CompletedTask;
    }
}