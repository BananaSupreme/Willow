using System.Diagnostics;

using Willow.DeviceAutomation.InputDevices.Abstractions;
using Willow.DeviceAutomation.InputDevices.Enums;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.MouseCommands;

internal sealed class ClickVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    public ClickVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public string InvocationPhrase => "click ?[[right|middle|left]:button]:_ ?[double]:doubleClick";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var button = context.Parameters.GetValueOrDefault("button")?.GetString() ?? "left";

        PerformClick(button);
        if (context.Parameters.TryGetValue("doubleClick", out _))
        {
            PerformClick(button);
        }

        return Task.CompletedTask;
    }

    private void PerformClick(string button)
    {
        switch (button)
        {
            case "left":
                _inputSimulator.Click();
                break;

            case "middle":
                _inputSimulator.Click(MouseButton.Middle);
                break;

            case "right":
                _inputSimulator.Click(MouseButton.Right);
                break;

            default:
                throw new UnreachableException();
        }
    }
}
