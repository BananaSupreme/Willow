using System.Diagnostics;

using Willow.BuiltInCommands.BasicEditing.Enums;
using Willow.DeviceAutomation.InputDevices;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.BasicEditing;

internal sealed class WriteFormattedVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    // ReSharper disable once UnusedMember.Local - used in command
    private readonly string[] _formatters = Enum.GetNames<Formatter>().Select(static x => x.ToLower()).ToArray();

    public WriteFormattedVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public string InvocationPhrase => "[_formatters]:formatter **phrase";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var words = context.Parameters["phrase"].GetString().Split(' ');
        var formatter = Enum.Parse<Formatter>(context.Parameters["formatter"].GetString(), true);
        var formattingFunction = GetFormattingFunction(formatter);

        _inputSimulator.Type(formattingFunction(words));
        return Task.CompletedTask;
    }

    private static Func<string[], string> GetFormattingFunction(Formatter formatter)
    {
        return formatter switch
        {
            Formatter.Upper => ToUpperFunction,
            Formatter.Lower => ToLowerFunction,
            Formatter.Camel => CamelCaseFunction,
            Formatter.Under => static words => "_" + CamelCaseFunction(words),
            Formatter.Dotted => static words => string.Join('.', words.Select(static x => x.ToLower())),
            Formatter.Pascal => static words => string.Join(string.Empty, words.Select(static x => Capitalize(x))),
            Formatter.Kebab => static words => string.Join('-', words.Select(static x => x.ToLower())),
            Formatter.Packed => static words => string.Join("::", words.Select(static x => x.ToLower())),
            Formatter.Slasher => static words => string.Join('/', words.Select(static x => x.ToLower())),
            Formatter.Smash => static words => string.Join(string.Empty, words.Select(static x => x.ToLower())),
            Formatter.Snake => static words => string.Join('_', words.Select(static x => x.ToLower())),
            Formatter.Title => static words => string.Join(' ', words.Select(static x => Capitalize(x))),
            Formatter.Sentence => SentenceCaseFunction,
            _ => throw new UnreachableException()
        };
    }

    private static string SentenceCaseFunction(string[] words)
    {
        return string.Join(' ', [Capitalize(words[0]), ..words[1..].Select(static word => word.ToLower())]);
    }

    private static string ToUpperFunction(string[] words)
    {
        return string.Join(' ', words.Select(static x => x.ToUpper()));
    }

    private static string ToLowerFunction(string[] words)
    {
        return string.Join(' ', words.Select(static x => x.ToLower()));
    }

    private static string CamelCaseFunction(string[] words)
    {
        return string.Join(' ', [words[0].ToLower(), ..words[1..].Select(static word => Capitalize(word))]);
    }

    private static string Capitalize(string word)
    {
        return new string([char.ToUpper(word[0]), ..word[1..]]);
    }
}
