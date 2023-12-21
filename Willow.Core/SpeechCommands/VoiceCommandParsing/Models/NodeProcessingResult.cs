using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;

namespace Willow.Core.SpeechCommands.VoiceCommandParsing.Models;

public readonly record struct NodeProcessingResult(
    bool IsSuccessful,
    CommandBuilder Builder,
    ReadOnlyMemory<Token> RemainingTokens);