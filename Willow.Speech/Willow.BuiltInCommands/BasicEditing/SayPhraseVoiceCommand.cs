using Willow.DeviceAutomation.InputDevices;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.BasicEditing;

internal sealed class SayPhraseVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    public SayPhraseVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public string InvocationPhrase => "[say|phrase]:_ **phrase";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        _inputSimulator.Type(context.Parameters["phrase"].GetString());
        return Task.CompletedTask;
    }
}
