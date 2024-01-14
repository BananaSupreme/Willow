using Willow.Core.Environment.Models;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.Abstractions;

/// <summary>
/// The structure that holds the available commands in the system and traverses using the input tokens. 
/// </summary>
internal interface ITrie
{
    /// <summary>
    /// Traverses the Trie structure.
    /// </summary>
    /// <param name="tokens">The input tokens.</param>
    /// <param name="tags">The current environment tags.</param>
    /// <returns>The result of the traversal.</returns>
    TrieTraversalResult TryTraverse(ReadOnlyMemory<Token> tokens, Tag[] tags);
}