using System.Diagnostics;

using Willow.BuiltInCommands.Helpers;
using Willow.DeviceAutomation.InputDevices;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.MouseCommands;

[ActivationMode([ActivationModeNames.Command, ActivationModeNames.Dictation])]
internal sealed class MouseDragVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    public MouseDragVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public string InvocationPhrase => "mouse [drag|release]:action";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var action = context.Parameters["action"].GetString();
        switch (action)
        {
            case "drag":
                _inputSimulator.MouseButtonDown();
                break;

            case "release":
                _inputSimulator.MouseButtonUp();
                break;

            default:
                throw new UnreachableException();
        }

        return Task.CompletedTask;
    }
}
