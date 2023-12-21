using Willow.Core.Helpers;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.NodeProcessors;

namespace Willow.Core.SpeechCommands.VoiceCommandCompilation.NodeCompilers;

internal sealed class WordNodeCompiler : INodeCompiler
{
    public (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                                      IDictionary<string, object> capturedValues,
                                                                      INodeCompiler[]
                                                                          specializedCommandParsers)
    {
        return commandWord.ContainsAnyExcept(CachedSearchValues.Alphabet)
                   ? INodeCompiler.Fail()
                   : (true, new WordNodeProcessor(new(commandWord.ToString())));
    }
}