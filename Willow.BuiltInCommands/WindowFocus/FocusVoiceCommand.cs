using Willow.DeviceAutomation.InputDevices;
using Willow.DeviceAutomation.InputDevices.Enums;
using Willow.Environment.Enums;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.WindowFocus;

[SupportedOss(SupportedOss.Windows)]
internal sealed class FocusVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    public FocusVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public string InvocationPhrase => "focus";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        _inputSimulator.PressKey(Key.LeftControl, Key.LeftAlt, Key.Tab);
        return Task.CompletedTask;
    }
}
