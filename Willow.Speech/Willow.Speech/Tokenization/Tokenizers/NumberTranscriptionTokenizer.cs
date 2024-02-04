using Willow.Speech.Tokenization.Consts;
using Willow.Speech.Tokenization.Models;
using Willow.Speech.Tokenization.Tokens;

namespace Willow.Speech.Tokenization.Tokenizers;

/// <summary>
/// Captures a numeric value
/// </summary>
internal sealed class NumberTranscriptionTokenizer : ITranscriptionTokenizer
{
    private readonly ILogger<NumberTranscriptionTokenizer> _log;

    public NumberTranscriptionTokenizer(ILogger<NumberTranscriptionTokenizer> log)
    {
        _log = log;
    }

    public TokenProcessingResult Process(ReadOnlySpan<char> input)
    {
        var wordEnd = input.IndexOf(Chars.Space);
        wordEnd = wordEnd < 0 ? input.Length : wordEnd;
        var word = input[..wordEnd];

        _log.ProcessedWord();
        return int.TryParse(word, out var num)
                   ? new TokenProcessingResult(true, new NumberToken(num), wordEnd)
                   : TokenProcessingResult.Failure;
    }
}

internal static partial class NumberTranscriptionTokenizerLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Trace, Message = "Value token processed")]
    public static partial void ProcessedWord(this ILogger logger);
}
