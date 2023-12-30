using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.VoiceCommandParsing.Eventing.Events;

public readonly record struct CommandParsedEvent(Guid Id, Dictionary<string, Token> Parameters);
