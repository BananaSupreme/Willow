using System.Collections.Frozen;

using Willow.Helpers.Logging.Extensions;
using Willow.Helpers.Logging.Loggers;
using Willow.Privacy.Settings;
using Willow.Settings;
using Willow.Speech.CommandExecution.Abstraction;
using Willow.Speech.CommandExecution.Exceptions;
using Willow.Speech.CommandExecution.Models;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Models;
using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.CommandExecution;

internal sealed class CommandStorage : ICommandStorage
{
    private readonly ILogger<CommandStorage> _log;
    private readonly ISettings<PrivacySettings> _privacySettings;

    private FrozenDictionary<Guid, Func<IVoiceCommand>> _storage
        = new Dictionary<Guid, Func<IVoiceCommand>>().ToFrozenDictionary();

    public CommandStorage(ILogger<CommandStorage> log, ISettings<PrivacySettings> privacySettings)
    {
        _log = log;
        _privacySettings = privacySettings;
    }

    public void AddCommands(ExecutableCommands[] commands)
    {
        _log.CommandsAdded(new EnumeratorLogger<Guid>(commands.Select(static x => x.Id)));
        var newCommands = commands.ToDictionary(static x => x.Id, static x => x.CommandActivator);
        _storage = _storage.Union(newCommands).ToFrozenDictionary();
    }

    public void RemoveCommands(ExecutableCommands[] commands)
    {
        _log.CommandsRemoved(new EnumeratorLogger<Guid>(commands.Select(static x => x.Id)));
        var newCommands = commands.ToDictionary(static x => x.Id, static x => x.CommandActivator);
        _storage = _storage.Except(newCommands).ToFrozenDictionary();
    }

    public async Task ExecuteCommandAsync(Guid id, VoiceCommandContext context)
    {
        using var ctx = _log.AddContext("commandId", id);
        _log.LookingForCommand();
        if (_storage.TryGetValue(id, out var commandActivator))
        {
            var command = commandActivator();
            _log.CommandMatched(
                new RedactingLogger<TypeNameLogger<IVoiceCommand>>(new TypeNameLogger<IVoiceCommand>(command),
                                                                   _privacySettings.CurrentValue.AllowLoggingCommands),
                new RedactingLogger<EnumeratorLogger<KeyValuePair<string, Token>>>(
                    new EnumeratorLogger<KeyValuePair<string, Token>>(context.Parameters),
                    _privacySettings.CurrentValue.AllowLoggingCommands));
            await command.ExecuteAsync(context);
            return;
        }

        _log.CommandNotFoundInStorage();
        throw new CommandNotFoundException();
    }
}

internal static partial class CommandStorageLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Added commands to storage: {commands}")]
    public static partial void CommandsAdded(this ILogger logger, EnumeratorLogger<Guid> commands);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Removed commands from storage: {commands}")]
    public static partial void CommandsRemoved(this ILogger logger, EnumeratorLogger<Guid> commands);

    [LoggerMessage(EventId = 3, Level = LogLevel.Trace, Message = "Looking for command")]
    public static partial void LookingForCommand(this ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Command was not found")]
    public static partial void CommandNotFoundInStorage(this ILogger logger);

    [LoggerMessage(EventId = 5,
                   Level = LogLevel.Information,
                   Message = "Command matched, executing ({commandName}) with parameters: {parameters}")]
    public static partial void CommandMatched(this ILogger logger,
                                              RedactingLogger<TypeNameLogger<IVoiceCommand>> commandName,
                                              RedactingLogger<EnumeratorLogger<KeyValuePair<string, Token>>> parameters);
}
