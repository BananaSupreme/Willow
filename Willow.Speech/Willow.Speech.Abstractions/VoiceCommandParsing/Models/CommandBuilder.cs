using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.VoiceCommandParsing.Models;

/// <summary>
/// Builder for a command.
/// </summary>
public sealed class CommandBuilder
{
    private CommandBuilder()
    {
    }

    /// <summary>
    /// The parameters captured within the command from the transcription.
    /// </summary>
    public Dictionary<string, Token> Parameters { get; } = [];

    /// <summary>
    /// The id of the command to be built.
    /// </summary>
    public Guid CommandId { get; private set; }

    /// <summary>
    /// Whether the command building process should be considered successful.
    /// </summary>
    public bool IsSuccessful { get; private set; }

    /// <summary>
    /// Builds a new instance of the <see cref="CommandBuilder" />.
    /// </summary>
    /// <returns>A fresh <see cref="CommandBuilder" /> instance.</returns>
    public static CommandBuilder Create()
    {
        return new CommandBuilder();
    }

    /// <summary>
    /// Adds a new parameter to the captured values in the command.
    /// </summary>
    /// <param name="name">Name of the parameter.</param>
    /// <param name="value">The value associated to the parameter.</param>
    public CommandBuilder AddParameter(string name, Token value)
    {
        Parameters[name] = value;
        return this;
    }

    /// <summary>
    /// Marks the command a success.
    /// </summary>
    public CommandBuilder Success()
    {
        IsSuccessful = true;
        return this;
    }

    /// <summary>
    /// A convince method that both marks the command successful and gives the command an Id.
    /// </summary>
    /// <param name="commandId">The command Id to associate with the command.</param>
    public CommandBuilder Success(Guid commandId)
    {
        CommandId = commandId;
        return Success();
    }

    /// <summary>
    /// Tries to build the command, successful if the command was marked a success before.
    /// </summary>
    /// <returns>
    /// Success of the run and the command that was parsed respectively, parsed command is meaningless when false.
    /// </returns>
    public (bool IsSuccessful, ParsedCommand Command) TryBuild()
    {
        if (!IsSuccessful)
        {
            return (false, default);
        }

        return (true, new ParsedCommand(CommandId, Parameters));
    }
}
