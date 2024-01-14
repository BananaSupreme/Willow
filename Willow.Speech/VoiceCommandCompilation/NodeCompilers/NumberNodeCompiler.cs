using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Extensions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandCompilation.NodeCompilers;

/// <summary>
/// Compiles the patterns
/// <code>
/// Number:variableName
/// N:variableName
/// #variableName
/// </code>
/// Those patterns are compiled into the <see cref="NumberNodeProcessor"/> with the variable in the captured values
/// being the name after the colon. <br/>
/// Meant to represent a number token.
/// </summary>
internal sealed class NumberNodeCompiler : INodeCompiler
{
    private static readonly char[][] _startSymbols = ["Number:".ToCharArray(), "N:".ToCharArray(), "#".ToCharArray()];

    public (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                                      IDictionary<string, object> capturedValues,
                                                                      INodeCompiler[]
                                                                          compilers)
    {
        var startSymbol = commandWord.FirstToStartWithOrNull(_startSymbols);
        if (startSymbol is null)
        {
            return INodeCompiler.Fail();
        }

        var captureValue = commandWord[startSymbol.Length..].GuardValidVariableName();

        return (true, new NumberNodeProcessor(captureValue.ToString()));
    }
}