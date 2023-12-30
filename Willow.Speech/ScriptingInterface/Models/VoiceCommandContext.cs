using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.ScriptingInterface.Models;

public readonly record struct VoiceCommandContext(Dictionary<string, Token> Parameters);