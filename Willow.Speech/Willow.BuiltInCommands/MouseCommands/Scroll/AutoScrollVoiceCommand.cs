using System.Diagnostics;
using System.Numerics;

using Willow.BuiltInCommands.MouseCommands.Scroll.Settings;
using Willow.DeviceAutomation.InputDevices;
using Willow.Environment;
using Willow.Environment.Models;
using Willow.Settings;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.MouseCommands.Scroll;

internal sealed class AutoScrollVoiceCommand : IVoiceCommand
{
    public const string AutoScrollingTagString = "<command>_auto_scroll";
    private static readonly Tag _autoScrollingTag = new(AutoScrollingTagString);
    private bool _running;
    private readonly IEnvironmentStateProvider _environmentStateProvider;
    private readonly IInputSimulator _inputSimulator;
    private readonly ISettings<ScrollSettings> _settings;

    public AutoScrollVoiceCommand(IInputSimulator inputSimulator,
                                  IEnvironmentStateProvider environmentStateProvider,
                                  ISettings<ScrollSettings> settings)
    {
        _inputSimulator = inputSimulator;
        _environmentStateProvider = environmentStateProvider;
        _settings = settings;
    }

    public string InvocationPhrase => "scroll auto ?[[start|stop]:action]:__ ?[[up|down]:direction]:_";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var action = context.Parameters.GetValueOrDefault("action")?.GetString() ?? "start";
        var direction = context.Parameters.GetValueOrDefault("direction")?.GetString() ?? "down";

        switch (action)
        {
            case "start":
                _ = Start(direction, _inputSimulator, _environmentStateProvider, _settings);
                break;

            case "stop":
                Stop(_environmentStateProvider);
                break;
        }

        return Task.CompletedTask;
    }

    private async Task Start(string direction,
                             IInputSimulator inputSimulator,
                             IEnvironmentStateProvider environmentStateProvider,
                             ISettings<ScrollSettings> settings)
    {
        environmentStateProvider.ActivateTag(_autoScrollingTag);
        if (_running)
        {
            return;
        }

        _running = true;
        while (_running)
        {
            inputSimulator.Scroll(GetFromDirection(direction));
            await Task.Delay(settings.CurrentValue.Speed);
        }
    }

    private void Stop(IEnvironmentStateProvider environmentStateProvider)
    {
        environmentStateProvider.DeactivateTag(_autoScrollingTag);
        _running = false;
    }

    [Tag(AutoScrollingTagString)]
    [VoiceCommand("?[scroll]:_ [faster|slower]:change")]
    public Task ChangeAutoScrollSpeed(VoiceCommandContext context)
    {
        var change = context.Parameters["change"].GetString();

        switch (change)
        {
            case "faster":
                _settings.Update(new ScrollSettings(Speed: _settings.CurrentValue.Speed - 10));
                break;
            case "slower":
                _settings.Update(new ScrollSettings(Speed: _settings.CurrentValue.Speed + 10));
                break;
            default:
                throw new UnreachableException();
        }

        return Task.CompletedTask;
    }

    private static Vector2 GetFromDirection(string direction)
    {
        return direction switch
        {
            "up" => new Vector2(0, -1),
            "down" => new Vector2(0, 1),
            _ => throw new UnreachableException()
        };
    }
}
