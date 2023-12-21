using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;

namespace Willow.Core.SpeechCommands.VoiceCommandParsing.Models;

public readonly record struct ParsedCommand(Guid Id, Dictionary<string, Token> Parameters);