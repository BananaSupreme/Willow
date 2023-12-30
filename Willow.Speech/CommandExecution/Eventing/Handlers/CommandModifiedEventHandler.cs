using Willow.Core.Eventing.Abstractions;
using Willow.Speech.CommandExecution.Abstraction;
using Willow.Speech.CommandExecution.Models;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Events;

namespace Willow.Speech.CommandExecution.Eventing.Handlers;

internal sealed class CommandModifiedEventHandler : IEventHandler<CommandModifiedEvent>
{
    private readonly ICommandStorage _commandStorage;

    public CommandModifiedEventHandler(ICommandStorage commandStorage)
    {
        _commandStorage = commandStorage;
    }

    public Task HandleAsync(CommandModifiedEvent @event)
    {
        var executableCommands = @event.Commands
                                       .Select(x => new ExecutableCommands(x.Id,
                                           (Func<IVoiceCommand>)x.CapturedValues[
                                               IVoiceCommand.CommandFunctionName]))
                                       .ToArray();
        _commandStorage.SetAvailableCommands(executableCommands);
        return Task.CompletedTask;
    }
}
