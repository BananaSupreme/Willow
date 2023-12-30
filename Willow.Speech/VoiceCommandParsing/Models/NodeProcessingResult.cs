using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.VoiceCommandParsing.Models;

public readonly record struct NodeProcessingResult(
    bool IsSuccessful,
    CommandBuilder Builder,
    ReadOnlyMemory<Token> RemainingTokens);