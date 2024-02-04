using Willow.Speech.VoiceCommandCompilation.Extensions;
using Willow.Speech.VoiceCommandParsing;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandCompilation.NodeCompilers;

/// <summary>
/// Compiles the patterns
/// <code>
/// And[...|...]
/// A[...|...]
/// &amp;[...|...]
/// </code>
/// Those patterns are compiled into the <see cref="AndNodeProcessor" />. <br/>
/// Meant to represent a group of patterns that must all be matched.
/// </summary>
internal sealed class AndNodeCompiler : INodeCompiler
{
    private static readonly char[][] _startSymbols = ["And".ToCharArray(), "A".ToCharArray(), "&".ToCharArray()];

    public (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                                      IDictionary<string, object> capturedValues,
                                                                      INodeCompiler[] compilers)
    {
        var startSymbol = commandWord.FirstToStartWithOrNull(_startSymbols);
        if (startSymbol is null)
        {
            return INodeCompiler.Fail();
        }

        var capturedValue = commandWord[startSymbol.Length..].GuardWrappedInSquares();
        var nodeProcessors = capturedValue.ExtractNodeProcessors(compilers, capturedValues);

        return (true, new AndNodeProcessor(nodeProcessors));
    }
}
