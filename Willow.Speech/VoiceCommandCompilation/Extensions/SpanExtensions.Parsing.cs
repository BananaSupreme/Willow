using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Exceptions;
using Willow.Speech.VoiceCommandParsing.Abstractions;

namespace Willow.Speech.VoiceCommandCompilation.Extensions;

internal static partial class SpanExtensions
{
    public static INodeProcessor ParseCommandWord(this ReadOnlySpan<char> word,
                                                  INodeCompiler[] compilers,
                                                  IDictionary<string, object> capturedValues)
    {
        foreach (var compiler in compilers)
        {
            var (isSuccessful, node) = compiler.TryParse(word, capturedValues, compilers);
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