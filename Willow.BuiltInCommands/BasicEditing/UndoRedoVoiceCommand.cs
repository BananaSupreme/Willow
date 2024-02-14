using System.Diagnostics;

using Willow.DeviceAutomation.InputDevices;
using Willow.DeviceAutomation.InputDevices.Enums;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.BasicEditing;

internal sealed class UndoRedoVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    public UndoRedoVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public string InvocationPhrase => "[undo|revert|redo|restore]:action ?[last]:_";

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
            "undo" => Key.Z,
            "revert" => Key.Z,
            "redo" => Key.Y,
            "restore" => Key.Y,
            _ => throw new UnreachableException()
        };
    }
}
