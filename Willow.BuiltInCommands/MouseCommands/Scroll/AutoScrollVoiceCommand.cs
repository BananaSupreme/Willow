using System.Diagnostics;
using System.Numerics;

using Willow.BuiltInCommands.MouseCommands.Scroll.Settings;
using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Models;
using Willow.Core.Settings.Abstractions;
using Willow.DeviceAutomation.InputDevices.Abstractions;
using Willow.Helpers.Locking;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.MouseCommands.Scroll;

internal sealed class AutoScrollVoiceCommand : IVoiceCommand
{
    public const string AutoScrollingTagString = "<command>_auto_scroll";
    private static readonly Tag _autoScrollingTag = new(AutoScrollingTagString);
    private readonly IInputSimulator _inputSimulator;
    private readonly IEnvironmentStateProvider _environmentStateProvider;
    private readonly ISettings<ScrollSettings> _settings;
    private static CancellationTokenSource? _cts;
    private static readonly DisposableLock _lock = new();


    public AutoScrollVoiceCommand(IInputSimulator inputSimulator, 
                                  IEnvironmentStateProvider environmentStateProvider,
                                  ISettings<ScrollSettings> settings)
    {
        _inputSimulator = inputSimulator;
        _environmentStateProvider = environmentStateProvider;
        _settings = settings;
    }

    public string InvocationPhrase => $"scroll auto ?[[start|stop]:action]:__ ?[[up|down]:direction]:_";

    public async Task ExecuteAsync(VoiceCommandContext context)
    {
        var action = context.Parameters.GetValueOrDefault("action")?.GetString() ?? "start";
        var direction = context.Parameters.GetValueOrDefault("direction")?.GetString() ?? "down";

        switch (action)
        {
            case "start":
                _ = Start(direction, _inputSimulator, _environmentStateProvider, _settings);
                break;

            case "stop":
                await Stop(_environmentStateProvider);
                break;
        }
    }

    private static async Task Start(string direction,
                                    IInputSimulator inputSimulator,
                                    IEnvironmentStateProvider environmentStateProvider,
                                    ISettings<ScrollSettings> settings)
    {
        using var unlocker = await _lock.LockAsync();

        if (_cts is not null)
        {
            return;
        }

        _cts = new();

        try
        {
            environmentStateProvider.AddTag(_autoScrollingTag);
            while (!_cts.IsCancellationRequested)
            {
                inputSimulator.Scroll(GetFromDirection(direction));
                await Task.Delay(settings.CurrentValue.Speed);
            }
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
        }
    }

    private static async Task Stop(IEnvironmentStateProvider environmentStateProvider)
    {
        if (_cts is not null)
        {
            environmentStateProvider.RemoveTag(_autoScrollingTag);
            await _cts.CancelAsync();
        }
    }

    private static Vector2 GetFromDirection(string direction)
    {
        return direction switch
        {
            "up" => new(0, -1),
            "down" => new(0, 1),
            _ => throw new UnreachableException()
        };
    }
}