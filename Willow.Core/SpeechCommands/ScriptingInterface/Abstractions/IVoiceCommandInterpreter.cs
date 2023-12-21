using Willow.Core.SpeechCommands.ScriptingInterface.Models;

namespace Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;

public interface IVoiceCommandInterpreter
{
    RawVoiceCommand InterpretCommand(IVoiceCommand voiceCommand);
}