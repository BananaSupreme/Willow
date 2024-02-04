using Willow.Environment.Models;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing;

internal sealed class Trie : ITrie
{
    public Node Root { get; }

    public Trie(Node root)
    {
        Root = root;
    }

    public TrieTraversalResult TryTraverse(ReadOnlyMemory<Token> tokens, Tag[] tags)
    {
        var (builder, remainingTokens) = Root.ProcessToken(tokens, CommandBuilder.Create(), tags);
        var (isSuccessful, command) = builder.TryBuild();
        return isSuccessful
                   ? new TrieTraversalResult(isSuccessful, command, remainingTokens)
                   : new TrieTraversalResult(isSuccessful, default, tokens);
    }
}
