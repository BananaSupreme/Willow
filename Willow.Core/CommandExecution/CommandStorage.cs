using System.Collections.Frozen;

using Willow.Core.CommandExecution.Abstraction;
using Willow.Core.CommandExecution.Exceptions;
using Willow.Core.CommandExecution.Models;
using Willow.Core.Helpers.Extensions;
using Willow.Core.Helpers.Logging;
using Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;
using Willow.Core.SpeechCommands.ScriptingInterface.Models;

namespace Willow.Core.CommandExecution;

internal class CommandStorage : ICommandStorage
{
    private readonly ILogger<CommandStorage> _log;

    private FrozenDictionary<Guid, Func<IVoiceCommand>> _storage =
        new Dictionary<Guid, Func<IVoiceCommand>>().ToFrozenDictionary();

    public CommandStorage(ILogger<CommandStorage> log)
    {
        _log = log;
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
            _log.CommandMatched();
            var command = commandActivator();
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
    public static partial void CommandsUpdated(this ILogger logger, LoggingEnumerator<Guid> commands);

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
        Level = LogLevel.Debug,
        Message = "Command matched, executing...")]
    public static partial void CommandMatched(this ILogger logger);
}