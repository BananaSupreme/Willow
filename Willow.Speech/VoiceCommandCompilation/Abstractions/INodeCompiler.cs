using Willow.Core.Environment.Models;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandCompilation.Abstractions;

public interface INodeCompiler
{
    (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                               IDictionary<string, object> capturedValues,
                                                               INodeCompiler[] compilers);

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