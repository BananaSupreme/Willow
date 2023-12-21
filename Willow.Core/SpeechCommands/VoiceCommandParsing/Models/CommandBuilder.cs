using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;

namespace Willow.Core.SpeechCommands.VoiceCommandParsing.Models;

public sealed class CommandBuilder
{
    public Dictionary<string, Token> Parameters { get; } = [];
    public Guid CommandId { get; private set; }
    public bool IsSuccessful { get; private set; }

    private CommandBuilder() { }


    public static CommandBuilder Create()
    {
        return new();
    }

    public CommandBuilder AddParameter(string name, Token value)
    {
        Parameters[name] = value;
        return this;
    }

    public CommandBuilder Success()
    {
        IsSuccessful = true;
        return this;
    }

    public CommandBuilder Success(Guid commandId)
    {
        CommandId = commandId;
        return Success();
    }

    public (bool IsSuccessful, ParsedCommand Command) TryBuild()
    {
        if (!IsSuccessful)
        {
            return (false, default);
        }

        return (true, new(CommandId, Parameters));
    }
}