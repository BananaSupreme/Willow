using System.Diagnostics;

using Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;
using Willow.Core.SpeechCommands.ScriptingInterface.Models;
using Willow.DeviceAutomation.InputDevices.Abstractions;

namespace Willow.BuiltInCommands.MouseCommands;

internal sealed class MouseDragVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    private const string _drag = "drag";
    private const string _release = "release";
    private const string _action = "action";
    public string InvocationPhrase => $"mouse [{_drag}|{_release}]:{_action}";

    public MouseDragVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var action = context.Parameters.GetValueOrDefault(_action)?.GetString() ?? throw new UnreachableException();
        switch (action)
        {
            case _drag:
                _inputSimulator.MouseButtonDown();
                break;
            
            case _release:
                _inputSimulator.MouseButtonUp();
                break;
            
            default:
                throw new UnreachableException();
        }

        return Task.CompletedTask;
    }
}