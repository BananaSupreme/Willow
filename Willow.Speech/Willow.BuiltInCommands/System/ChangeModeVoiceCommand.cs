using Willow.BuiltInCommands.Helpers;
using Willow.Environment;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.System;

[ActivationMode([ActivationModeNames.Command, ActivationModeNames.Dictation])]
internal sealed class ChangeModeVoiceCommand : IVoiceCommand
{
    private readonly IEnvironmentStateProvider _environmentStateProvider;

    public ChangeModeVoiceCommand(IEnvironmentStateProvider environmentStateProvider)
    {
        _environmentStateProvider = environmentStateProvider;
    }

    public string InvocationPhrase => "mode [command|dictation]:mode";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        _environmentStateProvider.SetActivationMode(context.Parameters["mode"].GetString());
        return Task.CompletedTask;
    }
}
