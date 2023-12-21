using Willow.Core.Environment.Models;
using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Models;

namespace Willow.Core.SpeechCommands.VoiceCommandParsing;

internal sealed class Trie : ITrie 
{
    private readonly Node _root;

    public Trie(Node root)
    {
        _root = root;
    }

    public (bool IsSuccessful, ParsedCommand Command, ReadOnlyMemory<Token> RemainingTokens) TryTraverse(ReadOnlyMemory<Token> tokens, Tag[] tags)
    {
        var (builder, remainingTokens) =  _root.ProcessToken(tokens, CommandBuilder.Create(), tags);
        var (isSuccessful, command) = builder.TryBuild();
        return (isSuccessful, command, remainingTokens);
    }
}