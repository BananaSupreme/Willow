using System.Collections.Frozen;

using Willow.Eventing;
using Willow.Helpers;
using Willow.Settings;
using Willow.Settings.Events;
using Willow.Speech.Tokenization.Abstractions;
using Willow.Speech.Tokenization.Consts;
using Willow.Speech.Tokenization.Enums;
using Willow.Speech.Tokenization.Models;
using Willow.Speech.Tokenization.Settings;
using Willow.Speech.Tokenization.Tokens;

namespace Willow.Speech.Tokenization.Tokenizers;

/// <summary>
/// Captures a word and allows to test for a homophone of it, or a close enough pronunciation.
/// </summary>
internal sealed class HomophonesTranscriptionTokenizer
    : ITranscriptionTokenizer, IEventHandler<SettingsUpdatedEvent<HomophoneSettings>>
{
    private readonly ISettings<HomophoneSettings> _settings;
    private readonly IHomophonesDictionaryLoader _homophonesDictionaryLoader;
    private Task? _dictionaryLoadingTask;
    private FrozenDictionary<string, string[]>? _homophones;

    public HomophonesTranscriptionTokenizer(ISettings<HomophoneSettings> settings,
                                            IHomophonesDictionaryLoader homophonesDictionaryLoader)
    {
        _settings = settings;
        _homophonesDictionaryLoader = homophonesDictionaryLoader;
        _dictionaryLoadingTask = LoadDictionaryAsync();
    }

    public Task HandleAsync(SettingsUpdatedEvent<HomophoneSettings> @event)
    {
        _dictionaryLoadingTask = LoadDictionaryAsync();
        return Task.CompletedTask;
    }

    public TokenProcessingResult Process(ReadOnlySpan<char> input)
    {
        var (word, length) = ProcessWord(input);
        if (word.Length == 0)
        {
            return TokenProcessingResult.Failure;
        }

        if (!_settings.CurrentValue.ShouldTestHomophones)
        {
            return ConvertTokenToResult(word, new WordToken(word), length);
        }

        var token = GetHomophonesToken(word);
        return ConvertTokenToResult(word, token, length);
    }

    private TokenProcessingResult ConvertTokenToResult(string word, WordToken token, int lengthProcessed)
    {
        return _settings.CurrentValue.UserDefinedHomophones.TryGetValue(word, out var homophones)
                   ? new TokenProcessingResult(true, new HomophonesToken(token, homophones), lengthProcessed)
                   : new TokenProcessingResult(true, token, lengthProcessed);
    }

    private WordToken GetHomophonesToken(string word)
    {
        return _settings.CurrentValue.HomophoneType switch
        {
            HomophoneType.CarnegieMelonDictionaryEquivalents => ProcessCarnegieMelonHomophones(word),
            HomophoneType.CarnegieMelonDictionaryNearEquivalents => ProcessCarnegieMelonHomophones(word),
            HomophoneType.Caverphone => new EncodingToken(word, WordEncoderType.Caverphone),
            HomophoneType.Metaphone => new EncodingToken(word, WordEncoderType.Metaphone),
        };
    }

    private WordToken ProcessCarnegieMelonHomophones(string word)
    {
        WordToken wordToken = new(word);
        //Loading the dictionary takes time, we shouldn't stop the user from reading, we will just ignore this for now.
        if ((_dictionaryLoadingTask is not null && !_dictionaryLoadingTask.IsCompleted) || _homophones is null)
        {
            return wordToken;
        }

        return _homophones.TryGetValue(word, out var homophones)
                   ? new HomophonesToken(wordToken, homophones)
                   : wordToken;
    }

    private static (string Word, int processed) ProcessWord(ReadOnlySpan<char> input)
    {
        var wordEnd = input.IndexOf(Chars.Space);
        wordEnd = wordEnd < 0 ? input.Length : wordEnd;
        var word = input[..wordEnd];
        return word.ContainsAnyExcept(CachedSearchValues.Alphabet) ? (string.Empty, 0) : (word.ToString(), wordEnd);
    }

    private async Task LoadDictionaryAsync()
    {
        await Task.Run(async () => _homophones = await _homophonesDictionaryLoader.LoadDictionaryAsync());
    }

    internal async Task FlushAsync()
    {
        if (_dictionaryLoadingTask is not null)
        {
            await _dictionaryLoadingTask;
        }
    }
}
