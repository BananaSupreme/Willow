using System.Diagnostics;

using Willow.BuiltInCommands.MouseCommands.Scroll.Settings;
using Willow.Core.Settings.Abstractions;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.MouseCommands.Scroll;

[Tag(AutoScrollVoiceCommand.AutoScrollingTagString)]
internal sealed class ChangeAutoScrollSpeedVoiceCommand : IVoiceCommand
{
    private readonly ISettings<ScrollSettings> _settings;

    public ChangeAutoScrollSpeedVoiceCommand(ISettings<ScrollSettings> settings)
    {
        _settings = settings;
    }

    public string InvocationPhrase => "?[scroll]:_ [faster|slower]:change";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var change = context.Parameters["change"].GetString();

        switch (change)
        {
            case "faster":
                _settings.Update(new(Speed: _settings.CurrentValue.Speed + 10));
                break;
            case "slower":
                _settings.Update(new(Speed: _settings.CurrentValue.Speed - 10));
                break;
            default:
                throw new UnreachableException();
        }
        return Task.CompletedTask;
    }
}