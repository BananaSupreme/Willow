using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Extensions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandCompilation.NodeCompilers;

internal sealed class WildCardNodeCompiler : INodeCompiler
{
    private static readonly char[][] _startSymbols = ["WildCard:".ToCharArray(), "*".ToCharArray()];
    private static readonly char[] _repeatingWildCardStartSymbols = "**".ToCharArray();

    public (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                                      IDictionary<string, object> capturedValues,
                                                                      INodeCompiler[]
                                                                          specializedCommandParsers)
    {
        var startSymbol = commandWord.FirstToStartWithOrNull(_startSymbols);
        if (startSymbol is null
            || commandWord.StartsWith(_repeatingWildCardStartSymbols))
        {
            return INodeCompiler.Fail();
        }

        var captureValue = commandWord[startSymbol.Length..].GuardValidVariableName();

        return (true, new WildCardNodeProcessor(captureValue.ToString()));
    }
}