using System.Diagnostics;

using Willow.DeviceAutomation.InputDevices.Abstractions;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.MouseCommands;

internal sealed class MoveMouseAbsoluteVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;
    
    private const string _horizontal = "horizontal";
    private const string _vertical = "vertical";
    public string InvocationPhrase => $"move mouse #{_horizontal} #{_vertical}";

    public MoveMouseAbsoluteVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }
    
    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var horizontal = context.Parameters.GetValueOrDefault(_horizontal)?.GetInt32() ??
                         throw new UnreachableException();
        var vertical = context.Parameters.GetValueOrDefault(_vertical)?.GetInt32() ??
                        throw new UnreachableException();

        _inputSimulator.MoveCursorToAbsolute(new(horizontal, vertical));
        return Task.CompletedTask;
    }
}