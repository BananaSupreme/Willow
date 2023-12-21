using Willow.Core.Helpers;
using Willow.Core.SpeechCommands.Tokenization.Consts;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Exceptions;

namespace Willow.Core.SpeechCommands.VoiceCommandCompilation.Extensions;

internal static partial class SpanExtensions
{
    public static ReadOnlySpan<char> GuardWrappedInSquares(this ReadOnlySpan<char> word)
    {
        var isValidStructure = word.Length > 2
                               && word[^1] == Chars.RightSquare
                               && word[0] == Chars.LeftSquare;

        return isValidStructure
                   ? word[1..^1]
                   : throw new CommandCompilationException("Node contained illeagel values");
    }

    public static ReadOnlySpan<char> GuardAlphabet(this ReadOnlySpan<char> word)
    {
        var isValidStructure = !word.IsEmpty
                               && !word.ContainsAnyExcept(CachedSearchValues.Alphabet);

        return isValidStructure
                   ? word
                   : throw new CommandCompilationException("Node contained illeagel values");
    }

    public static ReadOnlySpan<char> GuardValidVariableName(this ReadOnlySpan<char> word)
    {
        var isValidStructure = !word.IsEmpty
                               && CachedSearchValues.ValidVariableStarters.Contains(word[0])
                               && !word.ContainsAnyExcept(CachedSearchValues.ValidVariableCharacters);

        return isValidStructure
                   ? word
                   : throw new CommandCompilationException("Node contained illeagel values");
    }
}