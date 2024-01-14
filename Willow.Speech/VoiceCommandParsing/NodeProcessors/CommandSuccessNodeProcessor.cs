using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

/// <summary>
/// A leaf of command execution, represent a successful capturing of a command.
/// </summary>
/// <param name="CommandId">The id of the command.</param>
internal sealed record CommandSuccessNodeProcessor(Guid CommandId) : INodeProcessor
{
    public bool IsLeaf => true;
    public uint Weight => uint.MaxValue;

    public TokenProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder)
    {
        builder = builder.Success(CommandId);
        return new(true, builder, tokens);
    }
}