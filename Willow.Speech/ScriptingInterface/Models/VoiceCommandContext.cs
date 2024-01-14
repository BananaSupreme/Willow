using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.ScriptingInterface.Models;

/// <summary>
/// Context for voice command execution.
/// </summary>
/// <param name="Parameters">The parameters captured in the parsing of the command.</param>
public readonly record struct VoiceCommandContext(Dictionary<string, Token> Parameters);