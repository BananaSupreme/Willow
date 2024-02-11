using System.Diagnostics;

using Willow.DeviceAutomation.InputDevices;
using Willow.DeviceAutomation.InputDevices.Enums;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.BasicEditing;

[ActivationMode(activationMode: null)]
internal sealed class ZoomVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    public ZoomVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public string InvocationPhrase => "zoom [in|out|reset]:direction";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var key = context.Parameters["direction"].GetString() switch
        {
            "in" => Key.Plus,
            "out" => Key.Minus,
            "reset" => Key.Num0,
            _ => throw new UnreachableException()
        };

        _inputSimulator.PressKey(Key.LeftControl, key);
        return Task.CompletedTask;
    }
}
