using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.VoiceCommandParsing.Models;

/// <summary>
/// The result of traversing the Trie.
/// </summary>
/// <param name="IsSuccessful">Whether a command was found.</param>
/// <param name="Command">The command found, meaningless when not successful.</param>
/// <param name="RemainingTokens">The tokens left after the traversal, should be left unchanged in failure.</param>
public readonly record struct TrieTraversalResult(bool IsSuccessful,
                                                  ParsedCommand Command,
                                                  ReadOnlyMemory<Token> RemainingTokens);
