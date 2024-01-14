using Willow.Speech.ScriptingInterface.Models;

namespace Willow.Speech.ScriptingInterface.Abstractions;

/// <summary>
/// Interpreter for the <see cref="IVoiceCommand"/> interfaces converting them into objects recognized by the system.
/// </summary>
internal interface IVoiceCommandInterpreter
{
    /// <inheritdoc cref="IVoiceCommandInterpreter"/>
    /// <param name="voiceCommand">A voice command type.</param>
    /// <returns>A raw command that has not been processed.</returns>
    RawVoiceCommand InterpretCommand(IVoiceCommand voiceCommand);
}