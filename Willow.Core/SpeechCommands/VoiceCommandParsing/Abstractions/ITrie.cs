using Willow.Core.Environment.Models;
using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Models;

namespace Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;

public interface ITrie
{
    (bool IsSuccessful, ParsedCommand Command, ReadOnlyMemory<Token> RemainingTokens) TryTraverse(
        ReadOnlyMemory<Token> tokens, Tag[] tags);
}