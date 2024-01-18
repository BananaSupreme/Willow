using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

/// <summary>
/// A processor that succeeds whether the inner processor was successful or not.
/// </summary>
/// <param name="FlagName">This flag gets added to the command parameters when the inner processor succeeds.</param>
/// <param name="InnerNode">The inner processor to test.</param>
internal sealed record OptionalNodeProcessor(string FlagName, INodeProcessor InnerNode) : INodeProcessor
{
    public bool IsLeaf => InnerNode.IsLeaf;
    public uint Weight => InnerNode.Weight;

    public TokenProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder)
    {
        var (isSuccessful, innerBuilder, remainingTokens) = InnerNode.ProcessToken(tokens, builder);
        if (isSuccessful)
        {
            innerBuilder.AddParameter(FlagName, new EmptyToken());
            return new TokenProcessingResult(true, innerBuilder, remainingTokens);
        }

        return new TokenProcessingResult(true, builder, tokens);
    }
}
