using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Exceptions;
using Willow.Speech.VoiceCommandCompilation.Extensions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandCompilation.NodeCompilers;

/// <summary>
/// Compiles the patterns
/// <code>
/// RepeatingWildCard:variableName
/// RepeatingWildCard:variableName{number}
/// **variableName
/// **variableName{number}
/// </code>
/// Those patterns are compiled into the <see cref="RepeatingWildCardNodeProcessor" /> where the value is captured as
/// a single token into the variable name after the colon.<br/>
/// Optionally accepts a single integer, -1 (capture all) or larger than 0 representing the maximum amount of words to
/// be captured.<br/>
/// Meant to represent a capturing of many words.
/// </summary>
internal sealed class RepeatingWildCardNodeCompiler : INodeCompiler
{
    private static readonly char[][] _startSymbols = ["RepeatingWildCard:".ToCharArray(), "**".ToCharArray()];

    public (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                                      IDictionary<string, object> capturedValues,
                                                                      INodeCompiler[] compilers)
    {
        var startSymbol = commandWord.FirstToStartWithOrNull(_startSymbols);
        if (startSymbol is null)
        {
            return INodeCompiler.Fail();
        }

        var variables = commandWord.GetVariables();
        var count = -1;
        if (!variables.IsEmpty)
        {
            commandWord = commandWord.RemoveVariables(variables.Length);
            if (!int.TryParse(variables.ToString(), out count) || count < -1 || count == 0)
            {
                throw new CommandCompilationException(
                    "The only valid variable in a repeating wildcard is an int larger than 0 or -1 representing how many max words should be captured");
            }
        }

        commandWord = commandWord[startSymbol.Length..].GuardValidVariableName();

        return (true, new RepeatingWildCardNodeProcessor(commandWord.ToString(), count));
    }
}
