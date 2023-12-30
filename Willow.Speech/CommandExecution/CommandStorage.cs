using System.Collections.Frozen;

using Willow.Core.Logging.Extensions;
using Willow.Core.Logging.Loggers;
using Willow.Core.Logging.Settings;
using Willow.Speech.CommandExecution.Abstraction;
using Willow.Speech.CommandExecution.Exceptions;
using Willow.Speech.CommandExecution.Models;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Models;
using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.CommandExecution;

internal sealed class CommandStorage : ICommandStorage
{
    private readonly ILogger<CommandStorage> _log;
    private readonly IOptionsMonitor<PrivacySettings> _privacySettings;

    private FrozenDictionary<Guid, Func<IVoiceCommand>> _storage =
        new Dictionary<Guid, Func<IVoiceCommand>>().ToFrozenDictionary();

    public CommandStorage(ILogger<CommandStorage> log, IOptionsMonitor<PrivacySettings> privacySettings)
    {
        _log = log;
        _privacySettings = privacySettings;
    }

    public void SetAvailableCommands(ExecutableCommands[] commands)
    {
        _log.CommandsUpdated(new(commands.Select(x => x.Id)));
        _storage = commands.ToDictionary(x => x.Id, x => x.CommandActivator).ToFrozenDictionary();
    }

    public async Task ExecuteCommandAsync(Guid id, VoiceCommandContext context)
    {
        using var ctx = _log.AddContext("commandId", id);
        _log.LookingForCommand();
        if (_storage.TryGetValue(id, out var commandActivator))
        {
            var command = commandActivator();
            _log.CommandMatched(new(command.GetType().Name, _privacySettings.CurrentValue.AllowLoggingCommands),
                new(new(context.Parameters), _privacySettings.CurrentValue.AllowLoggingCommands));
            await command.ExecuteAsync(context);
            return;
        }

        _log.CommandNotFoundInStorage();
        throw new CommandNotFoundException();
    }
}

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Updated commands in storage: {commands}")]
    public static partial void CommandsUpdated(this ILogger logger, EnumeratorLogger<Guid> commands);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Trace,
        Message = "Looking for command")]
    public static partial void LookingForCommand(this ILogger logger);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "Command was not found")]
    public static partial void CommandNotFoundInStorage(this ILogger logger);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Command matched, executing ({commandName}) with parameters: {parameters}")]
    public static partial void CommandMatched(this ILogger logger, 
                                              RedactingLogger<string> commandName,
                                              RedactingLogger<EnumeratorLogger<KeyValuePair<string, Token>>> parameters);
}