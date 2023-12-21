using Willow.Core.SpeechCommands.VoiceCommandCompilation.Models;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;

namespace Willow.Core.SpeechCommands.VoiceCommandCompilation.Abstractions;

public interface ITrieFactory
{
    void Set(PreCompiledVoiceCommand[] commands);
    ITrie? Get();
}