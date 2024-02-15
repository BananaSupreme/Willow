using Willow.Speech.ScriptingInterface;
using Willow.Speech.Tokenization.Consts;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Exceptions;

namespace Willow.Speech.VoiceCommandCompilation.Extensions;

internal static partial class SpanExtensions
{
    /// <summary>
    /// Extracts single words consisting of only alphabet and no white space seperated by | char. <br/>
    /// word|another|word
    /// </summary>
    /// <param name="variables">The input to test.</param>
    /// <returns>Tokens matching the input.</returns>
    /// <exception cref="CommandCompilationException">The words consisted of none alphabet characters.</exception>
    public static Token[] ExtractFromInLineList(this ReadOnlySpan<char> variables)
    {
        var index = 0;
        List<Token> tokens = [];
        while (index > -1)
        {
            index = variables.IndexOf(Chars.Pipe);
            if (index == -1)
            {
                AddToken(variables, tokens);
            }
            else
            {
                var word = variables[..index];
                AddToken(word, tokens);
                variables = variables[(index + 1)..];
            }
        }

        var arrayTokens = tokens.ToArray();
        return arrayTokens;
    }

    private static void AddToken(ReadOnlySpan<char> words, List<Token> tokens)
    {
        if (words.Contains(Chars.Space))
        {
            tokens.Add(CreateMergedToken(words));
        }
        else
        {
            words.GuardAlphabet();
            tokens.Add(new WordToken(words.ToString()));
        }
    }

    private static Token CreateMergedToken(ReadOnlySpan<char> words)
    {
        var index = 0;
        List<WordToken> tokens = [];
        while (index > -1)
        {
            index = words.GetIndexOfNextSeparator(Chars.Space);
            switch (index)
            {
                case -1:
                    tokens.Add(new WordToken(words.ToString().Trim()));
                    break;
                case 0:
                    words = words[1..];
                    break;
                default:
                {
                    var word = words[..index];
                    tokens.Add(new WordToken(word.ToString().Trim()));
                    words = words[(index + 1)..];
                    break;
                }
            }
        }

        var finalTokens = tokens.Where(static x => x.Value != string.Empty).Cast<Token>().ToArray();
        return finalTokens.Length > 1 ? new MergedToken(finalTokens) : finalTokens[0];
    }

    /// <summary>
    /// Grabs the tokens that should be present in <paramref name="capturedValues" /> by their name.
    /// </summary>
    /// <param name="variables">The name of the variable to look inside <paramref name="capturedValues" />.</param>
    /// <param name="capturedValues">The values captured from <see cref="IVoiceCommand" /> file.</param>
    /// <returns>Tokens stored in the dictionary.</returns>
    /// <exception cref="CommandCompilationException">
    /// The variable did not exist inside <paramref name="capturedValues" /> or input was not a valid variable name.
    /// </exception>
    public static Token[] GetTokensFromCaptured(this ReadOnlySpan<char> variables,
                                                IDictionary<string, object> capturedValues)
    {
        variables.GuardValidVariableName();
        if (!capturedValues.TryGetValue(variables.ToString(), out var value))
        {
            throw new CommandCompilationException($"Unable to find captured list {variables}");
        }

        return value switch
        {
            IEnumerable<Token> tokens => tokens.ToArray(),
            IEnumerable<string> strings => strings.Select(static x => new WordToken(x)).Cast<Token>().ToArray(),
            _ => throw new CommandCompilationException(
                     $"expected the captured value ({variables}) to be an enumerable of types Token or string")
        };
    }
}
