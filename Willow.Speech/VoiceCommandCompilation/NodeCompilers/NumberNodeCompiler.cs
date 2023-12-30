using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Extensions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandCompilation.NodeCompilers;

internal sealed class NumberNodeCompiler : INodeCompiler
{
    private static readonly char[][] _startSymbols = ["Number:".ToCharArray(), "N:".ToCharArray(), "#".ToCharArray()];

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

        var captureValue = commandWord[startSymbol.Length..].GuardValidVariableName();

        return (true, new NumberNodeProcessor(captureValue.ToString()));
    }
}