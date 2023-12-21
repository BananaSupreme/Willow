using Willow.Core.Environment.Models;
using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Models;

namespace Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;

internal abstract record CapturingNodeProcessor(string CaptureName) : INodeProcessor
{
    public bool IsLeaf => false;
    public abstract uint Weight { get; }
    public virtual NodeProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder,
                                                     Tag[] environmentTags)
    {
        if (tokens.Length == 0)
        {
            return new(false, builder, tokens);
        }

        var token = tokens.Span[0];
        var matched = IsTokenMatch(token);
        if (matched)
        {
            builder = builder.AddParameter(CaptureName, token);
            return new(true, builder, tokens[1..]);
        }

        return new(false, builder, tokens);
    }

    protected abstract bool IsTokenMatch(Token token);
}