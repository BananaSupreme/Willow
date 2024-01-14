using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Willow.Speech.VoiceCommandCompilation.Abstractions;

/// <summary>
/// Compiles a section of command into a node processor that knows how to handle the incoming token correctly.<br/>
/// This is where the command syntax is actually created and implementing members registered with the DI are how the
/// system knows to convert command strings into processor arrays.
/// </summary>
/// <remarks>
/// This interface is registered automatically through assembly scanning, so any needed dependencies can be loaded via
/// the constructor.
/// </remarks>
public interface INodeCompiler
{
    /// <summary>
    /// Tries to compile a section of the command into a node processor.
    /// </summary>
    /// <param name="commandWord">
    /// A string representing a slice of the command between white space and the following white space.
    /// </param>
    /// <param name="capturedValues">Values captured from the <see cref="IVoiceCommand"/> interface.</param>
    /// <param name="compilers">All the compilers available in the system, for use when a compiler is a wrapper.</param>
    /// <returns>
    /// Whether the compilation was successful and the result of the compilation, when failed the ProcessedNode is
    /// meaningless.
    /// </returns>
    (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                               IDictionary<string, object> capturedValues,
                                                               INodeCompiler[] compilers);

    /// <summary>
    /// Convenience method to represent a failed processing.
    /// </summary>
    static (bool IsSuccefful, INodeProcessor ProccessedNode) Fail()
    {
        return (false, new FailedNodeProcessor());
    }

    public readonly struct FailedNodeProcessor : INodeProcessor
    {
        public bool IsLeaf => false;
        public uint Weight => 0;

        public TokenProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder)
        {
            throw new InvalidOperationException("This node processor should never be called");
        }
    }
}