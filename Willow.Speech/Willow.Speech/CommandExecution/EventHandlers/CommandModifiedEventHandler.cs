using Willow.Eventing;
using Willow.Speech.CommandExecution.Abstraction;
using Willow.Speech.CommandExecution.Models;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Eventing.Events;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.Speech.CommandExecution.EventHandlers;

/// <summary>
/// Loads the command into <see cref="ICommandStorage" /> when the available commands are modified.
/// </summary>
internal sealed class CommandModifiedEventHandler
    : IEventHandler<CommandsAddedEvent>, IEventHandler<CommandsRemovedEvent>
{
    private readonly ICommandStorage _commandStorage;

    public CommandModifiedEventHandler(ICommandStorage commandStorage)
    {
        _commandStorage = commandStorage;
    }

    public Task HandleAsync(CommandsAddedEvent @event)
    {
        var executableCommands = @event.Commands.ToExecutable();
        _commandStorage.AddCommands(executableCommands);
        return Task.CompletedTask;
    }

    public Task HandleAsync(CommandsRemovedEvent @event)
    {
        var executableCommands = @event.Commands.ToExecutable();
        _commandStorage.RemoveCommands(executableCommands);
        return Task.CompletedTask;
    }
}

file static class RawVoiceCommandExtensions
{
    public static ExecutableCommands[] ToExecutable(this RawVoiceCommand[] commands)
    {
        return commands
               .Select(static x => new ExecutableCommands(x.Id,
                                                          (Func<IVoiceCommand>)x.CapturedValues[
                                                              IVoiceCommand.CommandFunctionName]))
               .ToArray();
    }
}
