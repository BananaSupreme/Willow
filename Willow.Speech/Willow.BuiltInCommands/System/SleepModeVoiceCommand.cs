using Willow.BuiltInCommands.Helpers;
using Willow.Environment;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.System;

[Alias("go to sleep")]
[Alias("willow off")]
[Alias("stop listening")]
[ActivationMode(ActivationModeNames.Command, ActivationModeNames.Dictation)]
internal sealed class SleepModeVoiceCommand : IVoiceCommand
{
    private readonly IEnvironmentStateProvider _environmentStateProvider;
    public string InvocationPhrase => "mode sleep";

    public SleepModeVoiceCommand(IEnvironmentStateProvider environmentStateProvider)
    {
        _environmentStateProvider = environmentStateProvider;
    }

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        _environmentStateProvider.SetActivationMode("sleep");
        return Task.CompletedTask;
    }

    [Alias("wakeup")]
    [Alias("willow on")]
    [Alias("start listening")]
    [ActivationMode("sleep")]
    [VoiceCommand("mode command")]
    public Task WakeUpVoiceCommand(VoiceCommandContext context)
    {
        _environmentStateProvider.SetActivationMode(ActivationModeNames.Command);
        return Task.CompletedTask;
    }
}
