using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Models;
using Willow.Core.Eventing.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.SpeechToText.Eventing.Events;
using Willow.Core.SpeechCommands.Tokenization.Abstractions;
using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Eventing.Events;

namespace Willow.Core.SpeechCommands.VoiceCommandParsing.Eventing.Handlers;

internal sealed class AudioTranscribedEventHandler : IEventHandler<AudioTranscribedEvent>
{
    private readonly ITrieFactory _trieFactory;
    private readonly ITokenizer _tokenizer;
    private readonly IEnvironmentStateProvider _environmentStateProvider;
    private readonly IEventDispatcher _eventDispatcher;

    public AudioTranscribedEventHandler(ITrieFactory trieFactory, 
                                        ITokenizer tokenizer,
                                        IEnvironmentStateProvider environmentStateProvider,
                                        IEventDispatcher eventDispatcher)
    {
        _trieFactory = trieFactory;
        _tokenizer = tokenizer;
        _environmentStateProvider = environmentStateProvider;
        _eventDispatcher = eventDispatcher;
    }

    public Task HandleAsync(AudioTranscribedEvent @event)
    {
        var environment = _environmentStateProvider.Tags.ToArray();
        var tokens = _tokenizer.Tokenize(@event.Text);
        var baseCommandsTrie = _trieFactory.Get();
        if (baseCommandsTrie is null)
        {
            return Task.CompletedTask;
        }

        ProcessCommand(baseCommandsTrie, tokens, environment);

        return Task.CompletedTask;
    }

    private void ProcessCommand(ITrie baseCommandsTrie, ReadOnlyMemory<Token> tokens, Tag[] environment)
    {
        var (isSuccessful, command, remainingTokens) = baseCommandsTrie.TryTraverse(tokens, environment);
        if (isSuccessful)
        {
            _eventDispatcher.Dispatch(new CommandParsedEvent(command.Id, command.Parameters));
            ProcessCommand(baseCommandsTrie, remainingTokens, environment);
        }
    }
}