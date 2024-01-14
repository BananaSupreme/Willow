using Willow.Core.Environment.Models;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing;

internal sealed class Trie : ITrie
{
    private readonly Node _root;

    public Trie(Node root)
    {
        _root = root;
    }

    public TrieTraversalResult TryTraverse(ReadOnlyMemory<Token> tokens, Tag[] tags)
    {
        var (builder, remainingTokens) = _root.ProcessToken(tokens, CommandBuilder.Create(), tags);
        var (isSuccessful, command) = builder.TryBuild();
        return isSuccessful
                   ? new(isSuccessful, command, remainingTokens)
                   : new(isSuccessful, default, tokens);
    }
}