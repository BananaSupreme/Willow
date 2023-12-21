using Willow.Core.Eventing.Abstractions;
using Willow.Core.SpeechCommands.ScriptingInterface.Events;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Models;

namespace Willow.Core.SpeechCommands.VoiceCommandParsing.Eventing.Handlers;

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