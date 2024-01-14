using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.VoiceCommandParsing.Models;

/// <summary>
/// A command that was successfully parsed.
/// </summary>
/// <param name="Id">Id of the command.</param>
/// <param name="Parameters">The parameters captured in the parsing.</param>
public readonly record struct ParsedCommand(Guid Id, Dictionary<string, Token> Parameters);