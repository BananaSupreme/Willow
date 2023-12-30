using Willow.Core.Eventing.Abstractions;
using Willow.Speech.CommandExecution.Abstraction;
using Willow.Speech.VoiceCommandParsing.Eventing.Events;

namespace Willow.Speech.CommandExecution.Eventing.Handlers;

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