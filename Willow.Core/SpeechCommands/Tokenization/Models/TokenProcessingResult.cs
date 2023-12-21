using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;

namespace Willow.Core.SpeechCommands.Tokenization.Models;

public readonly record struct TokenProcessingResult(bool IsSuccessful, Token Token, int CharsProcessed);