using System.ComponentModel;

using Willow.Eventing;
using Willow.Speech.ScriptingInterface.Eventing.Events;
using Willow.Speech.VoiceCommandParsing.Events;

namespace Willow.BuiltInCommands.Repetition;

/// <Remark>
/// Public only to be able to be used in the voice commands, not supported for external consumption!
/// </Remark>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class LastCommandContainer : IEventHandler<CommandParsedEvent>, IEventHandler<CommandsAddedEvent>
{
    private Guid[] _idsOfRepetitionCommand = [];
    private static readonly string[] _repetitionCommandNames = ["Repeat", "Repeat Times", "Repeat Cardinal"];
    public CommandParsedEvent LastCommand { get; private set; }

    public Task HandleAsync(CommandParsedEvent @event)
    {
        if (!_idsOfRepetitionCommand.Contains(@event.Id))
        {
            LastCommand = @event;
        }

        return Task.CompletedTask;
    }

    public Task HandleAsync(CommandsAddedEvent @event)
    {
        var repetitionCommands = @event.Commands.Where(static x => _repetitionCommandNames.Contains(x.Name))
                                       .Select(static x => x.Id);
        _idsOfRepetitionCommand = _idsOfRepetitionCommand.Union(repetitionCommands).ToArray();

        return Task.CompletedTask;
    }
}
