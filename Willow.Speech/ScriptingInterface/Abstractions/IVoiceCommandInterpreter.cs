using Willow.Speech.ScriptingInterface.Models;

namespace Willow.Speech.ScriptingInterface.Abstractions;

public interface IVoiceCommandInterpreter
{
    RawVoiceCommand InterpretCommand(IVoiceCommand voiceCommand);
}