using Willow.Core.Helpers;
using Willow.Core.Logging.Loggers;
using Willow.Core.Logging.Settings;
using Willow.Speech.Tokenization.Abstractions;
using Willow.Speech.Tokenization.Consts;
using Willow.Speech.Tokenization.Models;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.Tokenization;

internal sealed class Tokenizer : ITokenizer
{
    private readonly ILogger<Tokenizer> _log;
    private readonly IOptionsMonitor<PrivacySettings> _privacySettings;
    private readonly ISpecializedTokenProcessor[] _specializedTokenProcessors;

    public Tokenizer(IEnumerable<ISpecializedTokenProcessor> specializedTokenProcessors, 
                     ILogger<Tokenizer> log,
                     IOptionsMonitor<PrivacySettings> privacySettings)
    {
        _log = log;
        _privacySettings = privacySettings;
        FallBackProcessor fallBackProcessor = new(log);
        _specializedTokenProcessors = [.. specializedTokenProcessors, fallBackProcessor];
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

        _log.TokenProcessingStarted(new(input, _privacySettings.CurrentValue.AllowLoggingTranscriptions));
        var index = inputSpan.IndexOfAny(CachedSearchValues.Alphanumeric);
        while (index > -1)
        {
            inputSpan = inputSpan[index..];
            inputSpan = ProcessSpan(inputSpan, tokens);
            index = inputSpan.IndexOfAny(CachedSearchValues.Alphanumeric);
        }

        _log.ProcessingCompleted(new(tokens, _privacySettings.CurrentValue.AllowLoggingTranscriptions));
        return [.. tokens];
    }

    private ReadOnlySpan<char> ProcessSpan(ReadOnlySpan<char> inputSpan, List<Token> tokens)
    {
        foreach (var specializedTokenProcessor in _specializedTokenProcessors)
        {
            var (isSuccessful, token, charsProcessed) = specializedTokenProcessor.Process(inputSpan);
            if (isSuccessful)
            {
                _log.TokenProcessed(new(token, _privacySettings.CurrentValue.AllowLoggingTranscriptions));
                tokens.Add(token);
                inputSpan = inputSpan[charsProcessed..];
                break;
            }
        }

        return inputSpan;
    }

    private sealed class FallBackProcessor : ISpecializedTokenProcessor
    {
        private readonly ILogger _log;

        public FallBackProcessor(ILogger log)
        {
            _log = log;
        }

        public TokenProcessingResult Process(ReadOnlySpan<char> input)
        {
            var wordEnd = input.IndexOf(Chars.Space);
            wordEnd = wordEnd < 0 ? input.Length : wordEnd;
            var word = input[..wordEnd];
            _log.ProcessedWord();
            return new(true, new WordToken(word.ToString()), wordEnd);
        }
    }
}

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Input detected was empty")]
    public static partial void EmptyInput(this ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Trace,
        Message = "Started processing tokens from input ({input})")]
    public static partial void TokenProcessingStarted(this ILogger logger, RedactingLogger<string> input);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Token processed successfully ({token})")]
    public static partial void TokenProcessed(this ILogger logger, RedactingLogger<Token> token);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Trace,
        Message = "Word token processed")]
    public static partial void ProcessedWord(this ILogger logger);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Completed processing of the transcription: {implementations}")]
    public static partial void ProcessingCompleted(this ILogger logger,
                                                   RedactingLogger<EnumeratorLogger<Token>> implementations);
}