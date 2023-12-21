using Willow.Core.SpeechCommands.VoiceCommandCompilation.Models;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;

namespace Willow.Core.SpeechCommands.VoiceCommandCompilation.Abstractions;

public interface IVoiceCommandCompiler
{
    INodeProcessor[] Compile(PreCompiledVoiceCommand command);
}