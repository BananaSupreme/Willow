using System;

namespace Willow.SourceGenerators;

internal static class TypeExtensions
{
    public static ReadOnlySpan<char> GetTypeNameWithoutVoiceCommandEndings(this ReadOnlySpan<char> name)
    {
        const string Command = "Command";
        const string VoiceCommand = "VoiceCommand";
        if (name.EndsWith(VoiceCommand.AsSpan()))
        {
            name = name.Slice(0, name.Length - VoiceCommand.Length);
        }
        else if (name.EndsWith(Command.AsSpan()))
        {
            name = name.Slice(0, name.Length - Command.Length);
        }

        return name;
    }
}
