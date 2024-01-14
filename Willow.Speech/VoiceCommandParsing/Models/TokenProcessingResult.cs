using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.VoiceCommandParsing.Models;

/// <summary>
/// The result of processing a token.
/// </summary>
/// <param name="IsSuccessful">Whether the processing was successful.</param>
/// <param name="Builder">The modified builder, should be unchanged if processing failed.</param>
/// <param name="RemainingTokens">
/// The tokens inputted without the tokens processed, should not change in failure.
/// </param>
public readonly record struct TokenProcessingResult(
    bool IsSuccessful,
    CommandBuilder Builder,
    ReadOnlyMemory<Token> RemainingTokens);