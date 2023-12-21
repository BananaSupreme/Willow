using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;

namespace Willow.Core.SpeechCommands.VoiceCommandParsing.Eventing.Events;

public readonly record struct CommandParsedEvent(Guid Id, Dictionary<string, Token> Parameters);
