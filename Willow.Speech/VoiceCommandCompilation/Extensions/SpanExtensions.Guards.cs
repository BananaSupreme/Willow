using Willow.Helpers;
using Willow.Speech.Tokenization.Consts;
using Willow.Speech.VoiceCommandCompilation.Exceptions;

namespace Willow.Speech.VoiceCommandCompilation.Extensions;

internal static partial class SpanExtensions
{
    /// <summary>
    /// Guards that the input is <c>"[...]"</c>.
    /// </summary>
    /// <param name="word">The input to test.</param>
    /// <returns>The input without the square brackets.</returns>
    /// <exception cref="CommandCompilationException">Throws if it does not fit the template.</exception>
    public static ReadOnlySpan<char> GuardWrappedInSquares(this ReadOnlySpan<char> word)
    {
        var isValidStructure = word.Length > 2 && word[^1] == Chars.RightSquare && word[0] == Chars.LeftSquare;

        return isValidStructure ? word[1..^1] : throw new CommandCompilationException("Node contained illegal values");
    }

    /// <summary>
    /// Guards that the input is none empty and does not contain none alphabet chars.
    /// </summary>
    /// <param name="word">Input to test.</param>
    /// <returns>The original input.</returns>
    /// <exception cref="CommandCompilationException">Throws if it does not fit the template.</exception>
    public static ReadOnlySpan<char> GuardAlphabet(this ReadOnlySpan<char> word)
    {
        var isValidStructure = !word.IsEmpty && !word.ContainsAnyExcept(CachedSearchValues.Alphabet);

        return isValidStructure ? word : throw new CommandCompilationException("Node contained illegal values");
    }

    /// <summary>
    /// Guards that the input only contains values valid as variables that is fit the following regex
    /// [a-zA-Z@_][a-zA-Z0-9@_]*
    /// </summary>
    /// <param name="word">Input to test.</param>
    /// <returns>The original input.</returns>
    /// <exception cref="CommandCompilationException">Throws if it does not fit the template.</exception>
    public static ReadOnlySpan<char> GuardValidVariableName(this ReadOnlySpan<char> word)
    {
        var isValidStructure = !word.IsEmpty
                               && CachedSearchValues.ValidVariableStarters.Contains(word[0])
                               && !word.ContainsAnyExcept(CachedSearchValues.ValidVariableCharacters);

        return isValidStructure ? word : throw new CommandCompilationException("Node contained illegal values");
    }
}
