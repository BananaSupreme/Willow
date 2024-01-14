using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Exceptions;
using Willow.Speech.VoiceCommandParsing.Abstractions;

namespace Willow.Speech.VoiceCommandCompilation.Extensions;

internal static partial class SpanExtensions
{
    /// <summary>
    /// Tests the input for compilation against the compilers.
    /// </summary>
    /// <param name="word">The input to test.</param>
    /// <param name="compilers">The compilers to test against.</param>
    /// <param name="capturedValues">The values captured from <see cref="IVoiceCommand"/> file.</param>
    /// <returns>The processor that matches the syntax.</returns>
    /// <exception cref="CommandCompilationException">
    /// Thrown if no compiler available matches the input word.
    /// </exception>
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

    /// <summary>
    /// Get the variables associated with the node, they would be found at the end of the declaration inside curly
    /// braces.  
    /// </summary>
    /// <param name="word">The input to look for the variables in.</param>
    /// <returns>The variables associated with the command word.</returns>
    /// <exception cref="CommandCompilationException">
    /// Throws if the open brace is found but the last character is not its closer.
    /// </exception>
    public static ReadOnlySpan<char> GetVariables(this ReadOnlySpan<char> word)
    {
        var variableStart = word.LastIndexOf('{');
        if (variableStart > 0)
        {
            if (word[^1] != '}')
            {
                throw new CommandCompilationException("Variables are expected at the end of the command word");
            }

            return word[(variableStart + 1)..^1];
        }

        return [];
    }

    /// <summary>
    /// Removes the variables from the input string, along with the braces.
    /// </summary>
    /// <param name="word">The input to clean.</param>
    /// <param name="variableLength">The length of the variables.</param>
    /// <returns>The input without the variables and associated braces.</returns>
    public static ReadOnlySpan<char> RemoveVariables(this ReadOnlySpan<char> word, int variableLength)
    {
        return word[..^(variableLength + 2)];
    }

    /// <summary>
    /// Searches for the first set of characters from the provided array of character arrays that the given word starts with.
    /// </summary>
    /// <param name="word">The word to be checked against the array of possible starting character sets.</param>
    /// <param name="possibleStartSymbols">An array of character arrays, each representing a possible set of starting characters to be matched against the word.</param>
    /// <returns>
    /// Returns the first set of characters from <paramref name="possibleStartSymbols"/> that the word starts with. If none of the character sets match the beginning of the word, returns null.
    /// </returns>
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