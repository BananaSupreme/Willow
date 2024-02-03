﻿using System.Diagnostics;
using System.Numerics;

using Willow.BuiltInCommands.MouseCommands.Scroll.Settings;
using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Models;
using Willow.Core.Settings.Abstractions;
using Willow.DeviceAutomation.InputDevices.Abstractions;
using Willow.Helpers.Locking;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.MouseCommands.Scroll;

internal sealed class AutoScrollVoiceCommand : IVoiceCommand
{
    public const string AutoScrollingTagString = "<command>_auto_scroll";
    private static readonly Tag _autoScrollingTag = new(AutoScrollingTagString);
    private static CancellationTokenSource? _cts;
    private static readonly DisposableLock _lock = new();
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
        if (_cts is not null || !_lock.CanEnter)
        {
            return;
        }

        using var unlocker = await _lock.LockAsync();

        _cts = new CancellationTokenSource();

        try
        {
            environmentStateProvider.ActivateTag(_autoScrollingTag);
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
            environmentStateProvider.DeactivateTag(_autoScrollingTag);
            await _cts.CancelAsync();
        }
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
