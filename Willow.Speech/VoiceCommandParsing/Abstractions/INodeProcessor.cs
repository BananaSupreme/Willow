using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandParsing.Abstractions;

/// <summary>
/// A processor that knows how to handle an incoming stream of tokens, extract relevant information from and mark,
/// successful when they are.
/// </summary>
public interface INodeProcessor
{
    /// <summary>
    /// Whether this is supposed to be a leaf node in the system, leaf nodes should never fail and mark the builder as
    /// successful.
    /// </summary>
    bool IsLeaf { get; }

    /// <summary>
    /// The weight of the processor, this allows the system to sort which processors should gain priority. <br/>
    /// A rule of thumb for deciding a weight should how specific is the capturing, for example: <br/>
    /// <see cref="WordNodeProcessor" /> has a weight of 1, since only one token can ever fit here. <br/>
    /// On the other hand, <see cref="RepeatingWildCardNodeProcessor" /> has positive infinity, since it will capture
    /// everything left.
    /// </summary>
    uint Weight { get; }

    /// <summary>
    /// Processes the incoming tokens and builds the command.
    /// </summary>
    /// <param name="tokens">The tokens that were not processed yet.</param>
    /// <param name="builder">The command builder.</param>
    /// <returns>The result of the processing.</returns>
    TokenProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder);
}
