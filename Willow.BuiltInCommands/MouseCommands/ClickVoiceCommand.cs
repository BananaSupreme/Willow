using System.Diagnostics;

using Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;
using Willow.Core.SpeechCommands.ScriptingInterface.Models;
using Willow.DeviceAutomation.InputDevices.Abstractions;
using Willow.DeviceAutomation.InputDevices.Enums;

namespace Willow.BuiltInCommands.MouseCommands;

internal sealed class ClickVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    private const string _right = "right";
    private const string _middle = "middle";
    private const string _left = "left";
    private const string _button = "button";
    private const string _doubleClick = "doubleClick";

    public ClickVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public string InvocationPhrase => $"click ?[[{_right}|{_middle}|{_left}]:{_button}]:_ ?[double]:{_doubleClick}";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var button = context.Parameters.GetValueOrDefault(_button)?.GetString() ?? _left;

        switch (button)
        {
            case _left:
                _inputSimulator.Click();
                break;
            
            case _middle:
                _inputSimulator.Click(MouseButton.Middle);
                break;
            
            case _right:
                _inputSimulator.Click(MouseButton.Right);
                break;
            
            default:
                throw new UnreachableException();
        }

        return Task.CompletedTask;
    }
}