using System.Diagnostics;

using Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;
using Willow.Core.SpeechCommands.ScriptingInterface.Models;
using Willow.DeviceAutomation.InputDevices.Abstractions;

namespace Willow.BuiltInCommands.MouseCommands;

internal class MoveMouseAbsoluteVoiceCommand : IVoiceCommand
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