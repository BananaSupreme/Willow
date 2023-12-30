using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.Tokenization.Models;

public readonly record struct TokenProcessingResult(bool IsSuccessful, Token Token, int CharsProcessed);