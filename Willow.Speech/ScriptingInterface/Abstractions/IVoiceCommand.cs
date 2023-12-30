using Willow.Speech.ScriptingInterface.Models;

namespace Willow.Speech.ScriptingInterface.Abstractions;

public interface IVoiceCommand
{
    public const string CommandFunctionName = "_command";

    string InvocationPhrase { get; }
    Task ExecuteAsync(VoiceCommandContext context);
}