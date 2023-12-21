using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;

namespace Willow.Core.SpeechCommands.ScriptingInterface.Models;

public readonly record struct VoiceCommandContext(Dictionary<string, Token> Parameters);