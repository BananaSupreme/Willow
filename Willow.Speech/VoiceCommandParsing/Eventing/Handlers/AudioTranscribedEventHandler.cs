using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Models;
using Willow.Core.Eventing.Abstractions;
using Willow.Speech.SpeechToText.Eventing.Events;
using Willow.Speech.Tokenization.Abstractions;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Eventing.Events;

namespace Willow.Speech.VoiceCommandParsing.Eventing.Handlers;

/// <summary>
/// Parses a transcription and executes all the consecutive commands in the input.
/// </summary>
internal sealed class AudioTranscribedEventHandler : IEventHandler<AudioTranscribedEvent>
{
    private readonly IEnvironmentStateProvider _environmentStateProvider;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly ITokenizer _tokenizer;
    private readonly ITrieFactory _trieFactory;

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
