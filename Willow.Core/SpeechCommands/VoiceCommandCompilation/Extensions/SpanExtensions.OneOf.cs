using Willow.Core.SpeechCommands.Tokenization.Consts;
using Willow.Core.SpeechCommands.Tokenization.Tokens;
using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Exceptions;

namespace Willow.Core.SpeechCommands.VoiceCommandCompilation.Extensions;

internal static partial class SpanExtensions
{
    public static Token[] ExtractFromInLineList(this ReadOnlySpan<char> variables)
    {
        var index = 0;
        List<Token> tokens = [];
        while (index > -1)
        {
            index = variables.IndexOf(Chars.Pipe);
            if (index == -1)
            {
                variables.GuardAlphabet();
                tokens.Add(new WordToken(variables.ToString()));
            }
            else
            {
                var word = variables[..index];
                word.GuardAlphabet();
                tokens.Add(new WordToken(word.ToString()));
                variables = variables[(index + 1)..];
            }
        }

        var arrayTokens = tokens.ToArray();
        return arrayTokens;
    }

    public static Token[] GetTokensFromCaptured(this ReadOnlySpan<char> variables,
                                                IDictionary<string, object> capturedValues)
    {
        variables.GuardValidVariableName();
        if (!capturedValues.TryGetValue(variables[1..].ToString(), out var value))
        {
            throw new CommandCompilationException($"Unable to find captured list {variables}");
        }

        if (value is IEnumerable<Token> tokens)
        {
            return tokens.ToArray();
        }

        if (value is IEnumerable<string> strings)
        {
            return strings.Select(x => new WordToken(x)).Cast<Token>().ToArray();
        }

        throw new CommandCompilationException(
            $"expected the captured value ({variables}) to be an enumerable of types Token or string");
    }
}