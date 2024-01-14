using Willow.Core.Eventing.Abstractions;
using Willow.Speech.ScriptingInterface.Eventing.Events;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Models;
using Willow.Speech.VoiceCommandParsing.Abstractions;

namespace Willow.Speech.VoiceCommandParsing.Eventing.Handlers;

/// <summary>
/// Triggers a rebuilt of the internal <see cref="ITrie"/> with the new command set.
/// </summary>
internal sealed class CommandModifiedEventHandler : IEventHandler<CommandModifiedEvent>
{
    private readonly ITrieFactory _trieFactory;

    public CommandModifiedEventHandler(ITrieFactory trieFactory)
    {
        _trieFactory = trieFactory;
    }

    public Task HandleAsync(CommandModifiedEvent @event)
    {
        var baseCommands = @event.Commands
                                 .SelectMany(x =>
                                 {
                                     return x.InvocationPhrases.Select(phrase =>
                                         new PreCompiledVoiceCommand(x.Id, phrase, x.TagRequirements,
                                             x.CapturedValues)
                                     );
                                 })
                                 .ToArray();
        _trieFactory.Set(baseCommands);

        return Task.CompletedTask;
    }
}