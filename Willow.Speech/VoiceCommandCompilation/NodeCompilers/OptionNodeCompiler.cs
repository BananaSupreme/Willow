using Willow.Speech.Tokenization.Consts;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Exceptions;
using Willow.Speech.VoiceCommandCompilation.Extensions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandCompilation.NodeCompilers;

internal sealed class OptionNodeCompiler : INodeCompiler
{
    private static readonly char[][] _startSymbols = ["Optional".ToCharArray(), "Opt".ToCharArray(), "?".ToCharArray()];

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

        var separatorIdx = commandWord.LastIndexOf(Chars.Colon);
        if (separatorIdx < 0)
        {
            throw new CommandCompilationException("Expected a flag name, but found none");
        }
        var captureValue = commandWord[startSymbol.Length..separatorIdx].GuardWrappedInSquares();
        var node = captureValue.ParseCommandWord(specializedCommandParsers, capturedValues);
        node.GuardNotSame<OptionalNodeProcessor>();
        
        commandWord = commandWord[(separatorIdx + 1)..];
        var flagName = commandWord.GuardValidVariableName();
        
        return (true, new OptionalNodeProcessor(node, flagName.ToString()));
    }
}