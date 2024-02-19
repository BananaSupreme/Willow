using System.Diagnostics;

using Willow.DeviceAutomation.InputDevices;
using Willow.DeviceAutomation.InputDevices.Enums;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.BasicEditing;

[ActivationMode(["command", "dictation"])]
internal sealed class CopyPasteVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    public CopyPasteVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public string InvocationPhrase => "[cut|copy|paste|clone]:action [that|here|this]:_";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var actionButton = GetActionButton(context.Parameters["action"].GetString());
        _inputSimulator.PressKey(Key.LeftCommandOrControl, actionButton);
        return Task.CompletedTask;
    }

    private static Key GetActionButton(string direction)
    {
        return direction switch
        {
            "cut" => Key.X,
            "copy" => Key.C,
            "paste" => Key.V,
            "clone" => Key.D,
            _ => throw new UnreachableException()
        };
    }
}
