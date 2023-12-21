using Willow.Core.SpeechCommands.Tokenization.Abstractions;
using Willow.Core.SpeechCommands.Tokenization.Consts;
using Willow.Core.SpeechCommands.Tokenization.Models;
using Willow.Core.SpeechCommands.Tokenization.Tokens;

namespace Willow.Core.SpeechCommands.Tokenization.Processors;

internal sealed class NumberTokenSpecializedProcessor : ISpecializedTokenProcessor
{
    private readonly ILogger<NumberTokenSpecializedProcessor> _log;

    public NumberTokenSpecializedProcessor(ILogger<NumberTokenSpecializedProcessor> log)
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
                   ? new(true, new NumberToken(num), wordEnd)
                   : ISpecializedTokenProcessor.Fail();
    }
}

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Trace,
        Message = "Word token processed")]
    public static partial void ProcessedWord(this ILogger logger);
}