using Willow.Core.Environment.Models;
using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Models;

namespace Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;

public interface INodeProcessor
{
    bool IsLeaf { get; }
    uint Weight { get; }
    NodeProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder, Tag[] environmentTags);
}