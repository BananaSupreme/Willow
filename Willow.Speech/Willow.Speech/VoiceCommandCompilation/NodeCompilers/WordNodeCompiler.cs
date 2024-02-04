using Willow.Helpers;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.VoiceCommandParsing;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandCompilation.NodeCompilers;

/// <summary>
/// Captures a specific word spoken. there is no specific pattern to this other than just an alphabet word.
/// </summary>
internal sealed class WordNodeCompiler : INodeCompiler
{
    public (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                                      IDictionary<string, object> capturedValues,
                                                                      INodeCompiler[] compilers)
    {
        return commandWord.ContainsAnyExcept(CachedSearchValues.Alphabet)
                   ? INodeCompiler.Fail()
                   : (true, new WordNodeProcessor(new WordToken(commandWord.ToString())));
    }
}
