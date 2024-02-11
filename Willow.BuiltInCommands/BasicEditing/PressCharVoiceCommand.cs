using System.Diagnostics;

using Willow.DeviceAutomation.InputDevices;
using Willow.DeviceAutomation.InputDevices.Enums;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;
using Willow.BuiltInCommands.BasicEditing.Enums;

namespace Willow.BuiltInCommands.BasicEditing;

internal sealed class PressCharVoiceCommand : IVoiceCommand
{
    private readonly IInputSimulator _inputSimulator;

    // ReSharper disable once UnusedMember.Local - used in command
    private readonly string[] _spokenAlphabets = Enum.GetNames<TalonAlphabet>()
                                                     .Union(Enum.GetNames<NatoAlphabet>())
                                                     .Select(static x => x.ToLower())
                                                     .ToArray();

    public PressCharVoiceCommand(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public string InvocationPhrase => "car *word";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        var keyWord = context.Parameters["word"].GetString();
        if (!Enum.TryParse<Key>(keyWord, true, out var key))
        {
            return Task.CompletedTask;
        }

        _inputSimulator.PressKey(key);
        return Task.CompletedTask;
    }

    [VoiceCommand("[_spokenAlphabets]:key", RequiredMethods = [nameof(ConvertAlphabetIdToKey)])]
    public Task PressCharPhoneticVoiceCommand(VoiceCommandContext context)
    {
        var keyWord = context.Parameters["key"].GetString();
        Key actualKey;

        if (Enum.TryParse<TalonAlphabet>(keyWord, true, out var talon))
        {
            actualKey = ConvertAlphabetIdToKey((int)talon);
        }
        else if (Enum.TryParse<NatoAlphabet>(keyWord, true, out var nato))
        {
            actualKey = ConvertAlphabetIdToKey((int)nato);
        }
        else
        {
            throw new UnreachableException();
        }

        _inputSimulator.PressKey(actualKey);
        return Task.CompletedTask;
    }

    private static Key ConvertAlphabetIdToKey(int id)
    {
        var asciiCode = id + 65;
        var asciiChar = (char)asciiCode;
        return Enum.Parse<Key>(asciiChar.ToString());
    }
}
