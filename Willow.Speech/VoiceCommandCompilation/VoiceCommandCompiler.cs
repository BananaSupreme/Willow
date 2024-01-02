using Willow.Helpers.Logging.Loggers;
using Willow.Speech.Tokenization.Consts;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Exceptions;
using Willow.Speech.VoiceCommandCompilation.Extensions;
using Willow.Speech.VoiceCommandCompilation.Models;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandCompilation;

internal sealed class VoiceCommandCompiler : IVoiceCommandCompiler
{
    private readonly IEnumerable<INodeCompiler> _internalParsers;
    private readonly ILogger<VoiceCommandCompiler> _log;

    public VoiceCommandCompiler(IEnumerable<INodeCompiler> internalParsers, ILogger<VoiceCommandCompiler> log)
    {
        _internalParsers = internalParsers;
        _log = log;
    }

    public INodeProcessor[] Compile(PreCompiledVoiceCommand command)
    {
        _log.CompilationStarted(command, new(_internalParsers.Select(x => x.GetType().Name)));
        if (string.IsNullOrWhiteSpace(command.InvocationPhrase))
        {
            throw new CommandCompilationException("Command string cannot be empty");
        }

        var commandSpan = command.InvocationPhrase.Trim().AsSpan();
        List<INodeProcessor> nodes = [];
        var index = 0;
        while (index > -1)
        {
            index = commandSpan.IndexOf(Chars.Space);
            var wordEnd = index > 0 ? index : commandSpan.Length;
            var word = commandSpan[..wordEnd];
            _log.ParsingWord(word.Length);
            var node = word.ParseCommandWord(_internalParsers.ToArray(), command.CapturedValues);
            _log.WordParsed(node, word.Length);
            nodes.Add(node);

            commandSpan = AdvanceSpan(commandSpan, wordEnd);
        }

        return [.. nodes, new CommandSuccessNodeProcessor(command.Id)];
    }

    private static ReadOnlySpan<char> AdvanceSpan(ReadOnlySpan<char> commandSpan, int wordEnd)
    {
        if (wordEnd != commandSpan.Length)
        {
            commandSpan = commandSpan[(wordEnd + 1)..];
        }

        return commandSpan;
    }
}

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Started compilation on command ({command}), compilers found: {nodeCompilers}")]
    public static partial void CompilationStarted(this ILogger logger, PreCompiledVoiceCommand command, EnumeratorLogger<string> nodeCompilers);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Trace,
        Message = "Started parsing word with length ({wordLength})")]
    public static partial void ParsingWord(this ILogger logger, int wordLength);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Word of length ({wordLength}) was parsed into node ({nodeProcessor})")]
    public static partial void WordParsed(this ILogger logger, INodeProcessor nodeProcessor, int wordLength);
}