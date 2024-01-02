using Willow.Helpers;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandCompilation.NodeCompilers;

internal sealed class WordNodeCompiler : INodeCompiler
{
    public (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                                      IDictionary<string, object> capturedValues,
                                                                      INodeCompiler[]
                                                                          compilers)
    {
        return commandWord.ContainsAnyExcept(CachedSearchValues.Alphabet)
                   ? INodeCompiler.Fail()
                   : (true, new WordNodeProcessor(new(commandWord.ToString())));
    }
}