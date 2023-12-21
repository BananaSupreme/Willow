using Willow.Core.SpeechCommands.Tokenization.Consts;
using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Exceptions;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Extensions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.NodeProcessors;

namespace Willow.Core.SpeechCommands.VoiceCommandCompilation.NodeCompilers;

internal sealed class OneOfSpecialSyntaxNodeCompiler : INodeCompiler
{
    private static readonly char[] _startSymbols = "[".ToCharArray();

    public (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                                      IDictionary<string, object> capturedValues,
                                                                      INodeCompiler[]
                                                                          specializedCommandParsers)
    {
        if (!commandWord.StartsWith(_startSymbols))
        {
            return INodeCompiler.Fail();
        }

        var separatorIdx = commandWord.IndexOf(Chars.Colon);
        var tokens = GetTokens(commandWord[..separatorIdx], capturedValues);

        commandWord = commandWord[(separatorIdx + 1)..];
        var captureValue = commandWord.GuardValidVariableName();

        return (true, new OneOfNodeProcessor(captureValue.ToString(), tokens));
    }

    private static Token[] GetTokens(ReadOnlySpan<char> variables, IDictionary<string, object> capturedValues)
    {
        variables = variables.GuardWrappedInSquares();
        if (variables.IsEmpty)
        {
            throw new CommandCompilationException(
                "OneOf must have a variable, either a reference to a captured list or a list syntax");
        }

        if (variables[0] == Chars.Underscore)
        {
            return variables.GetTokensFromCaptured(capturedValues);
        }

        return variables.ExtractFromInLineList();
    }
}