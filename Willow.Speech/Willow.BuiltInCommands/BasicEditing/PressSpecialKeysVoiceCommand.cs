using System.Diagnostics;

using Willow.DeviceAutomation.InputDevices;
using Willow.DeviceAutomation.InputDevices.Enums;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;
using Willow.BuiltInCommands.BasicEditing.Enums;

namespace Willow.BuiltInCommands.BasicEditing;

[ActivationMode(["command", "dictation"])]
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

    [VoiceCommand("[open|left]:_ [square|squares|object|curly|braces|brace|regular|parenthesis|angle]:type")]
    [ActivationMode(["command", "dictation"])]
    public Task OpenParenthesisVoiceCommand(VoiceCommandContext context)
    {
        var type = context.Parameters["type"].GetString();
        var key = type switch
        {
            "square" => Key.OpenBracket,
            "squares" => Key.OpenBracket,
            "object" => Key.OpenBrace,
            "curly" => Key.OpenBrace,
            "braces" => Key.OpenBrace,
            "brace" => Key.OpenBrace,
            "regular" => Key.OpenParenthesis,
            "parenthesis" => Key.OpenParenthesis,
            "angle" => Key.LessThan,
            _ => throw new UnreachableException()
        };
        _inputSimulator.PressKey(key);
        return Task.CompletedTask;
    }

    [VoiceCommand("[close|right]:_ [square|squares|object|curly|braces|brace|regular|parenthesis|angle]:type")]
    [ActivationMode(["command", "dictation"])]
    public Task CloseParenthesisVoiceCommand(VoiceCommandContext context)
    {
        var type = context.Parameters["type"].GetString();
        var key = type switch
        {
            "square" => Key.CloseBracket,
            "squares" => Key.CloseBracket,
            "object" => Key.CloseBrace,
            "curly" => Key.CloseBrace,
            "braces" => Key.CloseBrace,
            "brace" => Key.CloseBrace,
            "regular" => Key.CloseParenthesis,
            "parenthesis" => Key.CloseParenthesis,
            "angle" => Key.GreaterThan,
            _ => throw new UnreachableException()
        };
        _inputSimulator.PressKey(key);
        return Task.CompletedTask;
    }

    [VoiceCommand("[shift|ship|control|ctrl|alt|option|command]:control", RequiredMethods = [nameof(ConvertControlKey)])]
    [ActivationMode(["command", "dictation"])]
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

    [VoiceCommand("#number")]
    [ActivationMode(["command", "dictation"])]
    public Task PressNumberVoiceCommand(VoiceCommandContext context)
    {
        var number = context.Parameters["number"].GetInt32();
        if (number is >= 0 and <= 9)
        {
            var key = Enum.Parse<Key>($"Num{number}");
            _inputSimulator.PressKey(key);
        }

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
            SpecialKeys.Space => Key.Space,
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
            SpecialKeys.F1 => Key.F1,
            SpecialKeys.F2 => Key.F2,
            SpecialKeys.F3 => Key.F3,
            SpecialKeys.F4 => Key.F4,
            SpecialKeys.F5 => Key.F5,
            SpecialKeys.F6 => Key.F6,
            SpecialKeys.F7 => Key.F7,
            SpecialKeys.F8 => Key.F8,
            SpecialKeys.F9 => Key.F9,
            SpecialKeys.F10 => Key.F10,
            SpecialKeys.F11 => Key.F11,
            SpecialKeys.F12 => Key.F12,
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
        };
    }

    private static Key ConvertControlKey(string input)
    {
        return input switch
        {
            "shift" => Key.LeftShift,
            "ship" => Key.LeftShift,
            "ctrl" => Key.LeftCommandOrControl,
            "control" => Key.LeftCommandOrControl,
            "command" => Key.LeftCommandOrControl,
            "alt" => Key.LeftAltOrOption,
            "option" => Key.LeftAltOrOption,
            _ => throw new UnreachableException()
        };
    }
}
