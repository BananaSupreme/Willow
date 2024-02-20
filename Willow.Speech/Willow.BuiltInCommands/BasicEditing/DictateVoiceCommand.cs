using Willow.BuiltInCommands.Helpers;
using Willow.DeviceAutomation.InputDevices;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.BasicEditing;

[ActivationMode(ActivationModeNames.Dictation)]
internal sealed class DictateVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    public DictateVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public string InvocationPhrase => "**phrase";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        _inputSimulator.Type(context.Parameters["phrase"].GetString());
        return Task.CompletedTask;
    }
}
