using Willow.Core.SpeechCommands.VoiceCommandCompilation.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Exceptions;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Extensions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.NodeProcessors;

namespace Willow.Core.SpeechCommands.VoiceCommandCompilation.NodeCompilers;

internal sealed class RepeatingWildCardNodeCompiler : INodeCompiler
{
    private static readonly char[][] _startSymbols = ["RepeatingWildCard:".ToCharArray(), "**".ToCharArray()];

    public (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                                      IDictionary<string, object> capturedValues,
                                                                      INodeCompiler[]
                                                                          specializedCommandParsers)
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
            if (!int.TryParse(variables.ToString(), out count)
                || count < -1 || count == 0)
            {
                throw new CommandCompilationException(
                    "The only valid variable in a repeating wildcard is an int larger than 0 or -1 representing how many max words should be captured");
            }
        }

        commandWord = commandWord[startSymbol.Length..].GuardValidVariableName();

        return (true, new RepeatingWildCardNodeProcessor(commandWord.ToString(), count));
    }
}