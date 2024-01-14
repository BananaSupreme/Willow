using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.Tokenization.Models;

/// <summary>
/// Result of processing a token.
/// </summary>
/// <param name="IsSuccessful">Whether the processing is considered successful.</param>
/// <param name="Token">The token resulted.</param>
/// <param name="CharsProcessed">
/// The amount of characters processed by the processor, this is what will be removed after each processing run from,
/// the input string if the processing succeeds.
/// </param>
public readonly record struct TokenProcessingResult(bool IsSuccessful, Token Token, int CharsProcessed);