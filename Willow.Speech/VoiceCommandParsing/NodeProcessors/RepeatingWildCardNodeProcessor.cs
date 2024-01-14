using Willow.Speech.Tokenization.Consts;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandParsing.NodeProcessors;

/// <summary>
/// A processor that always succeeds, captures as many tokens as <paramref name="RepeatCount"/> requests, or all when -1.
/// </summary>
/// <param name="CaptureName">The variable name in the command parameters to capture the token.</param>
/// <param name="RepeatCount">The amount of variables to capture, -1 for all.</param>
internal sealed record RepeatingWildCardNodeProcessor(
    string CaptureName,
    int RepeatCount = -1) : INodeProcessor
{
    public bool IsLeaf => false;
    public uint Weight => uint.MaxValue;

    public TokenProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens,
                                             CommandBuilder builder)
    {
        if (tokens.Length == 0)
        {
            return new(false, builder, tokens);
        }

        var repeatCount = RepeatCount < 0 || tokens.Length > RepeatCount ? tokens.Length : RepeatCount;
        var tokensToProcess = tokens[..repeatCount].ToArray().Select(x => x.GetString());
        var mergedTokens = string.Join(Chars.Space, tokensToProcess);

        builder.AddParameter(CaptureName, new WordToken(mergedTokens));

        return repeatCount == tokens.Length
                   ? new(true, builder, ReadOnlyMemory<Token>.Empty)
                   : new(true, builder, tokens[repeatCount..]);
    }
}