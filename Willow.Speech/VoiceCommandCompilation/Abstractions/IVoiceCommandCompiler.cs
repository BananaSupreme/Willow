using Willow.Speech.VoiceCommandCompilation.Models;
using Willow.Speech.VoiceCommandParsing.Abstractions;

namespace Willow.Speech.VoiceCommandCompilation.Abstractions;

/// <summary>
/// Compiles a command into a the nodes that build it.
/// </summary>
internal interface IVoiceCommandCompiler
{
    /// <inheritdoc cref="IVoiceCommandCompiler"/>
    /// <param name="command">The input command to compile.</param>
    /// <returns>All the nodes that build the command, including the leaf.</returns>
    INodeProcessor[] Compile(PreCompiledVoiceCommand command);
}