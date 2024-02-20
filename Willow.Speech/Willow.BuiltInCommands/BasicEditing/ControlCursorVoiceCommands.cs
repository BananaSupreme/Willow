using System.Diagnostics;

using Willow.BuiltInCommands.Helpers;
using Willow.DeviceAutomation.InputDevices;
using Willow.DeviceAutomation.InputDevices.Enums;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.BasicEditing;

internal sealed class ControlCursorVoiceCommands
{
    private readonly IInputSimulator _inputSimulator;

    public ControlCursorVoiceCommands(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    [VoiceCommand("[go|move]:_ [left|right|up|down]:direction", RequiredMethods = [nameof(GetArrowKey)])]
    [ActivationMode([ActivationModeNames.Command, ActivationModeNames.Dictation])]
    public Task MoveCursorOneStepVoiceCommand(VoiceCommandContext context)
    {
        var arrow = GetArrowKey(context.Parameters["direction"].GetString());
        _inputSimulator.PressKey(arrow);
        return Task.CompletedTask;
    }

    [VoiceCommand("[go|move]:_ word [left|right]:direction", RequiredMethods = [nameof(GetArrowKey)])]
    [ActivationMode([ActivationModeNames.Command, ActivationModeNames.Dictation])]
    public Task MoveCursorWordVoiceCommand(VoiceCommandContext context)
    {
        var arrow = GetArrowKey(context.Parameters["direction"].GetString());
        _inputSimulator.PressKey(Key.LeftCommandOrControl, arrow);
        return Task.CompletedTask;
    }

    [VoiceCommand("[go|move]:_ line [start|head|end|tail]:direction", RequiredMethods = [nameof(GetHomeOrEndKey)])]
    [ActivationMode([ActivationModeNames.Command, ActivationModeNames.Dictation])]
    public Task MoveCursorEdgeOfLineVoiceCommand(VoiceCommandContext context)
    {
        var arrow = GetHomeOrEndKey(context.Parameters["direction"].GetString());
        _inputSimulator.PressKey(arrow);
        return Task.CompletedTask;
    }

    [VoiceCommand("[go|move]:_ file [start|head|end|tail]:direction", RequiredMethods = [nameof(GetHomeOrEndKey)])]
    [ActivationMode([ActivationModeNames.Command, ActivationModeNames.Dictation])]
    public Task MoveCursorEdgeOfPageVoiceCommand(VoiceCommandContext context)
    {
        var arrow = GetHomeOrEndKey(context.Parameters["direction"].GetString());
        _inputSimulator.PressKey(Key.LeftCommandOrControl, arrow);
        return Task.CompletedTask;
    }

    [VoiceCommand("select all")]
    [ActivationMode([ActivationModeNames.Command, ActivationModeNames.Dictation])]
    public Task SelectAllVoiceCommand(VoiceCommandContext context)
    {
        _inputSimulator.PressKey(Key.LeftCommandOrControl, Key.A);
        return Task.CompletedTask;
    }

    [VoiceCommand("select [left|right|up|down]:direction", RequiredMethods = [nameof(GetArrowKey)])]
    [ActivationMode([ActivationModeNames.Command, ActivationModeNames.Dictation])]
    public Task SelectOneStepVoiceCommand(VoiceCommandContext context)
    {
        var arrow = GetArrowKey(context.Parameters["direction"].GetString());
        _inputSimulator.PressKey(Key.LeftShift, arrow);
        return Task.CompletedTask;
    }

    [VoiceCommand("select word [left|right]:direction", RequiredMethods = [nameof(GetArrowKey)])]
    [ActivationMode([ActivationModeNames.Command, ActivationModeNames.Dictation])]
    public Task SelectWordVoiceCommand(VoiceCommandContext context)
    {
        var arrow = GetArrowKey(context.Parameters["direction"].GetString());
        _inputSimulator.PressKey(Key.LeftShift, Key.LeftCommandOrControl, arrow);
        return Task.CompletedTask;
    }

    [VoiceCommand("select line [start|head|end|tail]:direction", RequiredMethods = [nameof(GetHomeOrEndKey)])]
    [ActivationMode([ActivationModeNames.Command, ActivationModeNames.Dictation])]
    public Task SelectEdgeOfLineVoiceCommand(VoiceCommandContext context)
    {
        var arrow = GetHomeOrEndKey(context.Parameters["direction"].GetString());
        _inputSimulator.PressKey(Key.LeftShift, arrow);
        return Task.CompletedTask;
    }

    [VoiceCommand("select file [start|head|end|tail]:direction", RequiredMethods = [nameof(GetHomeOrEndKey)])]
    [ActivationMode([ActivationModeNames.Command, ActivationModeNames.Dictation])]
    public Task SelectEdgeOfPageVoiceCommand(VoiceCommandContext context)
    {
        var arrow = GetHomeOrEndKey(context.Parameters["direction"].GetString());
        _inputSimulator.PressKey(Key.LeftShift, Key.LeftCommandOrControl, arrow);
        return Task.CompletedTask;
    }

    [VoiceCommand("[clear|delete]:_ all", RequiredMethods = [nameof(SelectAllVoiceCommand)])]
    [ActivationMode([ActivationModeNames.Command, ActivationModeNames.Dictation])]
    public async Task DeleteAllVoiceCommand(VoiceCommandContext context)
    {
        await SelectAllVoiceCommand(context);
        _inputSimulator.PressKey(Key.Delete);
    }

    [VoiceCommand("[clear|delete]:_ [left|right|up|down]:direction",
                  RequiredMethods = [nameof(GetArrowKey), nameof(SelectOneStepVoiceCommand)])]
    [ActivationMode([ActivationModeNames.Command, ActivationModeNames.Dictation])]
    public async Task DeleteOneStepVoiceCommand(VoiceCommandContext context)
    {
        await SelectOneStepVoiceCommand(context);
        _inputSimulator.PressKey(Key.Delete);
    }

    [VoiceCommand("[clear|delete]:_ word [left|right]:direction",
                  RequiredMethods = [nameof(GetArrowKey), nameof(SelectWordVoiceCommand)])]
    [ActivationMode([ActivationModeNames.Command, ActivationModeNames.Dictation])]
    public async Task DeleteWordVoiceCommand(VoiceCommandContext context)
    {
        await SelectWordVoiceCommand(context);
        _inputSimulator.PressKey(Key.Delete);
    }

    [VoiceCommand("[clear|delete]:_ line [start|head|end|tail]:direction",
                  RequiredMethods = [nameof(GetHomeOrEndKey), nameof(SelectEdgeOfLineVoiceCommand)])]
    [ActivationMode([ActivationModeNames.Command, ActivationModeNames.Dictation])]
    public async Task DeleteEdgeOfLineVoiceCommand(VoiceCommandContext context)
    {
        await SelectEdgeOfLineVoiceCommand(context);
        _inputSimulator.PressKey(Key.Delete);
    }

    [VoiceCommand("[clear|delete]:_ file [start|head|end|tail]:direction",
                  RequiredMethods = [nameof(GetHomeOrEndKey), nameof(SelectEdgeOfPageVoiceCommand)])]
    [ActivationMode([ActivationModeNames.Command, ActivationModeNames.Dictation])]
    public async Task DeleteEdgeOfPageVoiceCommand(VoiceCommandContext context)
    {
        await SelectEdgeOfPageVoiceCommand(context);
        _inputSimulator.PressKey(Key.Delete);
    }

    private static Key GetHomeOrEndKey(string direction)
    {
        return direction switch
        {
            "start" => Key.Home,
            "head" => Key.Home,
            "end" => Key.End,
            "tail" => Key.End,
            _ => throw new UnreachableException()
        };
    }

    private static Key GetArrowKey(string direction)
    {
        return direction switch
        {
            "up" => Key.UpArrow,
            "down" => Key.DownArrow,
            "left" => Key.LeftArrow,
            "right" => Key.RightArrow,
            _ => throw new UnreachableException()
        };
    }
}
