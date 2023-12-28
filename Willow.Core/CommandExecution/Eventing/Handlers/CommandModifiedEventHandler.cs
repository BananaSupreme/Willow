using Willow.Core.CommandExecution.Abstraction;
using Willow.Core.CommandExecution.Models;
using Willow.Core.Eventing.Abstractions;
using Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;
using Willow.Core.SpeechCommands.ScriptingInterface.Events;

namespace Willow.Core.CommandExecution.Eventing.Handlers;

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
