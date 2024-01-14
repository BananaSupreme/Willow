using Willow.Speech.Tokenization.Consts;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Exceptions;
using Willow.Speech.VoiceCommandCompilation.Extensions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandCompilation.NodeCompilers;

/// <summary>
/// Compiles the patterns
/// <code>
/// Or[...|...]:indexName
/// O[...|...]:indexName
/// ~[...|...]:indexName
/// </code>
/// Those patterns are compiled into the <see cref="OrNodeProcessor"/>. where the index of the successful capture
/// is saved in the name of the index after the colon.<br/>
/// Meant to represent a group of patterns that only one must be matched.
/// </summary>
internal sealed class OrNodeCompiler : INodeCompiler
{
    private static readonly char[][] _startSymbols = ["Or".ToCharArray(), "O".ToCharArray(), "~".ToCharArray()];

    public (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                                      IDictionary<string, object> capturedValues,
                                                                      INodeCompiler[] compilers)
    {
        var startSymbol = commandWord.FirstToStartWithOrNull(_startSymbols);
        if (startSymbol is null)
        {
            return INodeCompiler.Fail();
        }

        var separatorIndex = commandWord.LastIndexOf(Chars.Colon);
        if (separatorIndex < 0)
        {
            throw new CommandCompilationException("Expected an index name, but found none");
        }
        
        var capturedValue = commandWord[startSymbol.Length..separatorIndex].GuardWrappedInSquares();
        var nodeProcessors = capturedValue.ExtractNodeProcessors(compilers, capturedValues);

        var successIndexName = commandWord[(separatorIndex + 1)..];
        successIndexName = successIndexName.GuardValidVariableName();
        
        return (true, new OrNodeProcessor(successIndexName.ToString(), nodeProcessors));
    }
}