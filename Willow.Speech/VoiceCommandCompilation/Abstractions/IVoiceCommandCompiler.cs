using Willow.Speech.VoiceCommandCompilation.Models;
using Willow.Speech.VoiceCommandParsing.Abstractions;

namespace Willow.Speech.VoiceCommandCompilation.Abstractions;

public interface IVoiceCommandCompiler
{
    INodeProcessor[] Compile(PreCompiledVoiceCommand command);
}