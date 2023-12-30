using Willow.Speech.VoiceCommandCompilation.Models;
using Willow.Speech.VoiceCommandParsing.Abstractions;

namespace Willow.Speech.VoiceCommandCompilation.Abstractions;

public interface ITrieFactory
{
    void Set(PreCompiledVoiceCommand[] commands);
    ITrie? Get();
}