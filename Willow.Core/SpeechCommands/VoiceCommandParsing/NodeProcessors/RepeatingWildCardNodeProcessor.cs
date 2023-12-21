using Willow.Core.Environment.Models;
using Willow.Core.SpeechCommands.Tokenization.Consts;
using Willow.Core.SpeechCommands.Tokenization.Tokens;
using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Models;

namespace Willow.Core.SpeechCommands.VoiceCommandParsing.NodeProcessors;

internal sealed record RepeatingWildCardNodeProcessor(
    string CaptureName,
    int RepeatCount = -1) : CapturingNodeProcessor(CaptureName)
{
    public override uint Weight => uint.MaxValue;

    public override NodeProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder,
                                                      Tag[] environmentTags)
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

    protected override bool IsTokenMatch(Token token)
    {
        throw new NotImplementedException();
    }
}