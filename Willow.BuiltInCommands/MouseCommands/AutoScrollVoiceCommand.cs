using System.Diagnostics;
using System.Numerics;

using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Models;
using Willow.Core.Helpers;
using Willow.DeviceAutomation.InputDevices.Abstractions;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.MouseCommands;

internal sealed class AutoScrollVoiceCommand : IVoiceCommand
{
    public static readonly Tag ScrollingTag = new("__scrolling");
    private readonly IInputSimulator _inputSimulator;
    private readonly IEnvironmentStateProvider _environmentStateProvider;
    private static CancellationTokenSource? _cts;
    private static readonly DisposableLock _lock = new();


    public AutoScrollVoiceCommand(IInputSimulator inputSimulator, IEnvironmentStateProvider environmentStateProvider)
    {
        _inputSimulator = inputSimulator;
        _environmentStateProvider = environmentStateProvider;
    }

    public string InvocationPhrase => $"scroll auto ?[[start|stop]:action]:__ ?[[up|down]:direction]:_";

    public async Task ExecuteAsync(VoiceCommandContext context)
    {
        var action = context.Parameters.GetValueOrDefault("action")?.GetString() ?? "start";
        var direction = context.Parameters.GetValueOrDefault("direction")?.GetString() ?? "down";

        switch (action)
        {
            case "start":
                _ = Start(direction, _inputSimulator, _environmentStateProvider);
                break;

            case "stop":
                await Stop(_environmentStateProvider);
                break;
        }
    }

    private static async Task Start(string direction, 
                                    IInputSimulator inputSimulator,
                                    IEnvironmentStateProvider environmentStateProvider)
    {
        using var unlocker = await _lock.LockAsync();

        if (_cts is not null)
        {
            return;
        }

        _cts = new();

        try
        {
            environmentStateProvider.AddTag(ScrollingTag);
            while (!_cts.IsCancellationRequested)
            {
                inputSimulator.Scroll(GetFromDirection(direction));
                await Task.Delay(100);
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
            environmentStateProvider.RemoveTag(ScrollingTag);
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