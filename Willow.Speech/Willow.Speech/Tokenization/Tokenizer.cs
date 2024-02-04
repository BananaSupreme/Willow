using Willow.Helpers;
using Willow.Helpers.Logging.Loggers;
using Willow.Privacy.Settings;
using Willow.Registration;
using Willow.Settings;
using Willow.Speech.Tokenization.Abstractions;
using Willow.Speech.Tokenization.Consts;
using Willow.Speech.Tokenization.Models;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.Tokenization;

internal sealed class Tokenizer : ITokenizer
{
    private readonly ILogger<Tokenizer> _log;
    private readonly ISettings<PrivacySettings> _privacySettings;
    private readonly ICollectionProvider<ITranscriptionTokenizer> _specializedNodeProcessors;
    private readonly FallBackTranscriptionTokenizer _fallBackTranscriptionTokenizer;

    public Tokenizer(ICollectionProvider<ITranscriptionTokenizer> specializedNodeProcessors,
                     ILogger<Tokenizer> log,
                     ISettings<PrivacySettings> privacySettings)
    {
        _specializedNodeProcessors = specializedNodeProcessors;
        _log = log;
        _privacySettings = privacySettings;
        _fallBackTranscriptionTokenizer = new FallBackTranscriptionTokenizer(log);
    }

    public Token[] Tokenize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _log.EmptyInput();
            return [];
        }

        var inputSpan = input.AsSpan();
        var tokens = new List<Token>();

        _log.TokenProcessingStarted(
            new RedactingLogger<string>(input, _privacySettings.CurrentValue.AllowLoggingTranscriptions));
        var index = inputSpan.IndexOfAny(CachedSearchValues.Alphanumeric);
        while (index > -1)
        {
            inputSpan = inputSpan[index..];
            inputSpan = ProcessSpan(inputSpan, tokens);
            index = inputSpan.IndexOfAny(CachedSearchValues.Alphanumeric);
        }

        _log.ProcessingCompleted(
            new RedactingLogger<EnumeratorLogger<Token>>(tokens,
                                                         _privacySettings.CurrentValue.AllowLoggingTranscriptions));
        return [.. tokens];
    }

    private ReadOnlySpan<char> ProcessSpan(ReadOnlySpan<char> inputSpan, List<Token> tokens)
    {
        var allNodeProcessors = _specializedNodeProcessors.Get().Append(_fallBackTranscriptionTokenizer);
        foreach (var specializedNodeProcessor in allNodeProcessors)
        {
            var (isSuccessful, token, charsProcessed) = specializedNodeProcessor.Process(inputSpan);
            if (isSuccessful)
            {
                _log.TokenProcessed(
                    new RedactingLogger<Token>(token, _privacySettings.CurrentValue.AllowLoggingTranscriptions));
                tokens.Add(token);
                inputSpan = inputSpan[charsProcessed..];
                break;
            }
        }

        return inputSpan;
    }

    private sealed class FallBackTranscriptionTokenizer : ITranscriptionTokenizer
    {
        private readonly ILogger _log;

        public FallBackTranscriptionTokenizer(ILogger log)
        {
            _log = log;
        }

        public TokenProcessingResult Process(ReadOnlySpan<char> input)
        {
            var wordEnd = input.IndexOf(Chars.Space);
            wordEnd = wordEnd < 0 ? input.Length : wordEnd;
            var word = input[..wordEnd];
            _log.ProcessedWord();
            return new TokenProcessingResult(true, new WordToken(word.ToString()), wordEnd);
        }
    }
}

internal static partial class TokenizerLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Input detected was empty")]
    public static partial void EmptyInput(this ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Trace, Message = "Started processing tokens from input ({input})")]
    public static partial void TokenProcessingStarted(this ILogger logger, RedactingLogger<string> input);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Token processed successfully ({token})")]
    public static partial void TokenProcessed(this ILogger logger, RedactingLogger<Token> token);

    [LoggerMessage(EventId = 4, Level = LogLevel.Trace, Message = "Value token processed")]
    public static partial void ProcessedWord(this ILogger logger);

    [LoggerMessage(EventId = 5,
                   Level = LogLevel.Information,
                   Message = "Completed processing of the transcription: {implementations}")]
    public static partial void ProcessingCompleted(this ILogger logger,
                                                   RedactingLogger<EnumeratorLogger<Token>> implementations);
}
