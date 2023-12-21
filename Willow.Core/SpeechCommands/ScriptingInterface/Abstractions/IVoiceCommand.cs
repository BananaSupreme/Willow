using Willow.Core.SpeechCommands.ScriptingInterface.Models;

namespace Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;

public interface IVoiceCommand
{
    public const string CommandFunctionName = "_command";

    string InvocationPhrase { get; }
    Task ExecuteAsync(VoiceCommandContext context);
}