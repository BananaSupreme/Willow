using Willow.Core.Environment.Models;
using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Models;

namespace Willow.Core.SpeechCommands.VoiceCommandCompilation.Abstractions;

public interface INodeCompiler
{
    (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                               IDictionary<string, object> capturedValues,
                                                               INodeCompiler[] specializedCommandParsers);

    static (bool IsSuccefful, INodeProcessor ProccessedNode) Fail()
    {
        return (false, new FailedNodeProcessor());
    }

    public class FailedNodeProcessor : INodeProcessor
    {
        public bool IsLeaf => false;
        public uint Weight => 0;

        public NodeProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder,
                                                 Tag[] environmentTags)
        {
            throw new InvalidOperationException("This node processor should never be called");
        }
    }
}