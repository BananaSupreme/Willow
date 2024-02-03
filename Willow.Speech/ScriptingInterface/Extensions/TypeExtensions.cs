namespace Willow.Speech.ScriptingInterface.Extensions;

internal static class TypeExtensions
{
    public static ReadOnlySpan<char> GetTypeNameWithoutVoiceCommandEndings(this ReadOnlySpan<char> name)
    {
        const string Command = "Command";
        const string VoiceCommand = "VoiceCommand";
        if (name.EndsWith(VoiceCommand))
        {
            name = name[..^VoiceCommand.Length];
        }
        else if (name.EndsWith(Command))
        {
            name = name[..^Command.Length];
        }

        return name;
    }
}
