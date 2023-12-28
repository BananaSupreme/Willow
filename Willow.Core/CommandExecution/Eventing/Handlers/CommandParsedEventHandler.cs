using Willow.Core.CommandExecution.Abstraction;
using Willow.Core.Eventing.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Eventing.Events;

namespace Willow.Core.CommandExecution.Eventing.Handlers;

internal sealed class CommandParsedEventHandler : IEventHandler<CommandParsedEvent>
{
    private readonly ICommandStorage _commandStorage;

    public CommandParsedEventHandler(ICommandStorage commandStorage)
    {
        _commandStorage = commandStorage;
    }

    public async Task HandleAsync(CommandParsedEvent @event)
    {
        await _commandStorage.ExecuteCommandAsync(@event.Id, new(@event.Parameters));
    }
}