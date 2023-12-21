using Willow.Core.SpeechCommands.VoiceCommandCompilation.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Exceptions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;

namespace Willow.Core.SpeechCommands.VoiceCommandCompilation.Extensions;

internal static partial class SpanExtensions
{
    public static INodeProcessor ParseCommandWord(this ReadOnlySpan<char> word,
                                                  INodeCompiler[] internalParsers,
                                                  IDictionary<string, object> capturedValues)
    {
        foreach (var internalParser in internalParsers)
        {
            var (isSuccessful, node) = internalParser.TryParse(word, capturedValues, internalParsers);
            if (isSuccessful)
            {
                return node;
            }
        }

        throw new CommandCompilationException($"Unable to parse command {word}");
    }

    public static ReadOnlySpan<char> GetVariables(this ReadOnlySpan<char> word)
    {
        var variableStart = word.LastIndexOf('{');
        if (variableStart > 0)
        {
            var variableEnd = word.LastIndexOf('}');
            return variableEnd > variableStart
                       ? word[(variableStart + 1)..variableEnd]
                       : throw new CommandCompilationException("Variable declaration left open");
        }

        return [];
    }

    public static ReadOnlySpan<char> RemoveVariables(this ReadOnlySpan<char> word, int variableLength)
    {
        return word[..^(variableLength + 2)];
    }

    public static char[]? FirstToStartWithOrNull(this ReadOnlySpan<char> word, char[][] possibleStartSymbols)
    {
        foreach (var symbols in possibleStartSymbols)
        {
            if (word.StartsWith(symbols))
            {
                return symbols;
            }
        }

        return null;
    }
}