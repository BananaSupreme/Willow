using Willow.Core.Environment.Models;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.Abstractions;

public interface ITrie
{
    (bool IsSuccessful, ParsedCommand Command, ReadOnlyMemory<Token> RemainingTokens) TryTraverse(
        ReadOnlyMemory<Token> tokens, Tag[] tags);
}