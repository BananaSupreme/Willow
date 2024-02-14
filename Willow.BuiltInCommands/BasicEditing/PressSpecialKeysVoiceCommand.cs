﻿using System.Diagnostics;

using Willow.DeviceAutomation.InputDevices;
using Willow.DeviceAutomation.InputDevices.Enums;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;
using Willow.BuiltInCommands.BasicEditing.Enums;

namespace Willow.BuiltInCommands.BasicEditing;

[ActivationMode(activationMode: null)]
internal sealed class PressSpecialKeysVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    // ReSharper disable once UnusedMember.Local - used in command
    private readonly string[] _keys = Enum.GetNames<SpecialKeys>().Select(static x => x.ToLower()).ToArray();

    public PressSpecialKeysVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public string InvocationPhrase => "[_keys]:key";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var special = context.Parameters["key"].GetString();
        special = new string([char.ToUpper(special[0]), ..special[1..]]);
        var key = Enum.Parse<SpecialKeys>(special);
        var actualKey = MatchKey(key);
        _inputSimulator.PressKey(actualKey);
        return Task.CompletedTask;
    }

    [VoiceCommand("[open|left]:_ [square|squares|object|curly|regular|parenthesis|angle]:type")]
    [ActivationMode(activationMode: null)]
    public Task OpenParenthesisVoiceCommand(VoiceCommandContext context)
    {
        var type = context.Parameters["type"].GetString();
        var key = type switch
        {
            "square" => Key.OpenBracket,
            "squares" => Key.OpenBracket,
            "object" => Key.OpenBrace,
            "curly" => Key.OpenBrace,
            "regular" => Key.OpenParenthesis,
            "parenthesis" => Key.OpenParenthesis,
            "angle" => Key.LessThan,
            _ => throw new UnreachableException()
        };
        _inputSimulator.PressKey(key);
        return Task.CompletedTask;
    }

    [VoiceCommand("[close|right]:_ [square|squares|object|curly|regular|parenthesis|angle]:type")]
    [ActivationMode(activationMode: null)]
    public Task CloseParenthesisVoiceCommand(VoiceCommandContext context)
    {
        var type = context.Parameters["type"].GetString();
        var key = type switch
        {
            "square" => Key.CloseBracket,
            "squares" => Key.CloseBracket,
            "object" => Key.CloseBrace,
            "curly" => Key.CloseBrace,
            "regular" => Key.CloseParenthesis,
            "parenthesis" => Key.CloseParenthesis,
            "angle" => Key.GreaterThan,
            _ => throw new UnreachableException()
        };
        _inputSimulator.PressKey(key);
        return Task.CompletedTask;
    }

    [VoiceCommand("[shift|ship|control|alt|option|command]:control", RequiredMethods = [nameof(ConvertControlKey)])]
    [ActivationMode(activationMode: null)]
    public Task PressControlKeyVoiceCommand(VoiceCommandContext context)
    {
        var control = ConvertControlKey(context.Parameters["control"].GetString());
        _inputSimulator.KeyDown(control);
        _ = Task.Run(async () =>
        {
            await Task.Delay(300);
            _inputSimulator.KeyUp(control);
        });
        return Task.CompletedTask;
    }

    private static Key MatchKey(SpecialKeys key)
    {
        return key switch
        {
            SpecialKeys.Tab => Key.Tab,
            SpecialKeys.Left => Key.LeftArrow,
            SpecialKeys.Right => Key.RightArrow,
            SpecialKeys.Up => Key.UpArrow,
            SpecialKeys.Down => Key.DownArrow,
            SpecialKeys.Home => Key.Home,
            SpecialKeys.End => Key.End,
            SpecialKeys.Delete => Key.Delete,
            SpecialKeys.Enter => Key.Enter,
            SpecialKeys.Backspace => Key.Backspace,
            SpecialKeys.Insert => Key.Insert,
            SpecialKeys.Escape => Key.Escape,
            SpecialKeys.Dollar => Key.Dollar,
            SpecialKeys.Percent => Key.Percent,
            SpecialKeys.Minus => Key.Minus,
            SpecialKeys.Plus => Key.Plus,
            SpecialKeys.Equals => Key.Equal,
            SpecialKeys.Colon => Key.Colon,
            SpecialKeys.Semicolon => Key.Semicolon,
            SpecialKeys.Quotation => Key.Quotation,
            SpecialKeys.Ampersand => Key.Ampersand,
            SpecialKeys.Backslash => Key.Backslash,
            SpecialKeys.Slash => Key.Slash,
            SpecialKeys.Dot => Key.Dot,
            SpecialKeys.Period => Key.Dot,
            SpecialKeys.Question => Key.Question,
            SpecialKeys.Multiply => Key.Asterisk,
            SpecialKeys.Divide => Key.Slash,
            SpecialKeys.Star => Key.Asterisk,
            SpecialKeys.Tilde => Key.Tilde,
            SpecialKeys.Exclamation => Key.Exclamation,
            SpecialKeys.Bang => Key.Exclamation,
            SpecialKeys.At => Key.At,
            SpecialKeys.Hash => Key.Hash,
            SpecialKeys.Caret => Key.Caret,
            SpecialKeys.Asterisk => Key.Asterisk,
            SpecialKeys.Equal => Key.Equal,
            SpecialKeys.Underscore => Key.Underscore,
            SpecialKeys.Pipe => Key.Pipe,
            SpecialKeys.Apostrophe => Key.Apostrophe,
            SpecialKeys.Comma => Key.Comma,
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
        };
    }

    private static Key ConvertControlKey(string input)
    {
        return input switch
        {
            "shift" => Key.LeftShift,
            "ship" => Key.LeftShift,
            "control" => Key.LeftCommandOrControl,
            "command" => Key.LeftCommandOrControl,
            "alt" => Key.LeftAltOrOption,
            "option" => Key.LeftAltOrOption,
            _ => throw new UnreachableException()
        };
    }
}
