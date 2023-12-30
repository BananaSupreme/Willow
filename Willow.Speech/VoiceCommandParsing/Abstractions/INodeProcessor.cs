using Willow.Core.Environment.Models;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.Abstractions;

public interface INodeProcessor
{
    bool IsLeaf { get; }
    uint Weight { get; }
    NodeProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder, Tag[] environmentTags);
}