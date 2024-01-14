using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.Tokenization.Consts;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Exceptions;
using Willow.Speech.VoiceCommandCompilation.Extensions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandCompilation.NodeCompilers;

/// <summary>
/// Compiles the patterns
/// <code>
/// OneOf:variableName{capturedVariableName}
/// OneOf:variableName{[..|..]}
/// </code>
/// Those patterns are compiled into the <see cref="OneOfNodeProcessor"/> with the variable in the captured values
/// being the name after the colon. and the values captured being either a field or property named in the curly braces
/// from the <see cref="IVoiceCommand"/> file or a list of words seperated by the | character.<br/>
/// Meant to represent a group of words that are all valid in this command.
/// </summary>
internal sealed class OneOfNodeCompiler : INodeCompiler
{
    private static readonly char[] _startSymbols = "OneOf:".ToCharArray();

    public (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                                      IDictionary<string, object> capturedValues,
                                                                      INodeCompiler[] compilers)
    {
        if (!commandWord.StartsWith(_startSymbols))
        {
            return INodeCompiler.Fail();
        }

        var variables = commandWord.GetVariables();
        var tokens = GetTokens(variables, capturedValues);

        commandWord = commandWord.RemoveVariables(variables.Length);
        var captureValue = commandWord[_startSymbols.Length..];

        return (true, new OneOfNodeProcessor(captureValue.ToString(), tokens));
    }

    private static Token[] GetTokens(ReadOnlySpan<char> variables, IDictionary<string, object> capturedValues)
    {
        if (variables.IsEmpty)
        {
            throw new CommandCompilationException(
                "OneOf must have a variable, either a refernece to a captured list or a list syntax");
        }

        if (variables[0] == Chars.Underscore)
        {
            return variables.GetTokensFromCaptured(capturedValues);
        }

        variables = variables.GuardWrappedInSquares();
        return variables.ExtractFromInLineList();
    }
}