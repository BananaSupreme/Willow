using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.VoiceCommandParsing.Models;

public readonly record struct ParsedCommand(Guid Id, Dictionary<string, Token> Parameters);