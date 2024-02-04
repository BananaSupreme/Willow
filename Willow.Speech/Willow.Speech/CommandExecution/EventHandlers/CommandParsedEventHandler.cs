using Willow.Eventing;
using Willow.Speech.CommandExecution.Abstraction;
using Willow.Speech.ScriptingInterface.Models;
using Willow.Speech.VoiceCommandParsing.Events;

namespace Willow.Speech.CommandExecution.EventHandlers;

/// <summary>
/// Executes the parsed command.
/// </summary>
internal sealed class CommandParsedEventHandler : IEventHandler<CommandParsedEvent>
{
    private readonly ICommandStorage _commandStorage;

    public CommandParsedEventHandler(ICommandStorage commandStorage)
    {
        _commandStorage = commandStorage;
    }

    public async Task HandleAsync(CommandParsedEvent @event)
    {
        await _commandStorage.ExecuteCommandAsync(@event.Id, new VoiceCommandContext(@event.Parameters));
    }
}
