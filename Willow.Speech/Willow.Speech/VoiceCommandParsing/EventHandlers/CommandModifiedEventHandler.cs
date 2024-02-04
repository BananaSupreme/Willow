using Willow.Eventing;
using Willow.Speech.ScriptingInterface.Eventing.Events;
using Willow.Speech.ScriptingInterface.Models;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Models;

namespace Willow.Speech.VoiceCommandParsing.EventHandlers;

/// <summary>
/// Manages the current collection of events in the system, against the <see cref="ITrieFactory"/>.
/// </summary>
internal sealed class CommandModifiedEventHandler
    : IEventHandler<CommandReconstructionRequestedEvent>,
      IEventHandler<CommandsAddedEvent>,
      IEventHandler<CommandsRemovedEvent>
{
    private readonly ITrieFactory _trieFactory;
    private readonly HashSet<PreCompiledVoiceCommand> _commands = [];

    public CommandModifiedEventHandler(ITrieFactory trieFactory)
    {
        _trieFactory = trieFactory;
    }

    public Task HandleAsync(CommandReconstructionRequestedEvent @event)
    {
        _trieFactory.Set(_commands.ToArray());
        return Task.CompletedTask;
    }

    public Task HandleAsync(CommandsAddedEvent @event)
    {
        var preCompiled = @event.Commands.ToPreCompiled();
        foreach (var preCompiledVoiceCommand in preCompiled)
        {
            _commands.Add(preCompiledVoiceCommand);
        }

        _trieFactory.Set(_commands.ToArray());
        return Task.CompletedTask;
    }

    public Task HandleAsync(CommandsRemovedEvent @event)
    {
        var preCompiled = @event.Commands.ToPreCompiled();
        foreach (var preCompiledVoiceCommand in preCompiled)
        {
            _commands.Remove(preCompiledVoiceCommand);
        }

        _trieFactory.Set(_commands.ToArray());
        return Task.CompletedTask;
    }
}

file static class RawVoiceCommandExtensions
{
    public static PreCompiledVoiceCommand[] ToPreCompiled(this RawVoiceCommand[] commands)
    {
        return commands.SelectMany(static x =>
                       {
                           return x.InvocationPhrases.Select(
                               phrase => new PreCompiledVoiceCommand(
                                   x.Id,
                                   phrase,
                                   x.TagRequirements,
                                   x.CapturedValues));
                       })
                       .ToArray();
    }
}
