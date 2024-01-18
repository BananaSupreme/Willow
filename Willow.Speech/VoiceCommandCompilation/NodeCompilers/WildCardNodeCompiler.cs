using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Extensions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandCompilation.NodeCompilers;

/// <summary>
/// Compiles the patterns
/// <code>
/// WildCard:variableName
/// *variableName
/// </code>
/// Those patterns are compiled into the <see cref="WildCardNodeProcessor" /> where the value is captured into the
/// variable name after the colon.<br/>
/// Meant to represent a capturing of any word.
/// </summary>
internal sealed class WildCardNodeCompiler : INodeCompiler
{
    private static readonly char[][] _startSymbols = ["WildCard:".ToCharArray(), "*".ToCharArray()];
    private static readonly char[] _repeatingWildCardStartSymbols = "**".ToCharArray();

    public (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                                      IDictionary<string, object> capturedValues,
                                                                      INodeCompiler[] compilers)
    {
        var startSymbol = commandWord.FirstToStartWithOrNull(_startSymbols);
        if (startSymbol is null || commandWord.StartsWith(_repeatingWildCardStartSymbols))
        {
            return INodeCompiler.Fail();
        }

        var captureValue = commandWord[startSymbol.Length..].GuardValidVariableName();

        return (true, new WildCardNodeProcessor(captureValue.ToString()));
    }
}
