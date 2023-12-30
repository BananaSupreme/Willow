using Willow.Core.Environment.Models;
using Willow.Core.Helpers;
using Willow.Core.Logging.Extensions;
using Willow.Core.Logging.Loggers;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Models;
using Willow.Speech.VoiceCommandParsing;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandCompilation;

internal sealed class TrieFactory : ITrieFactory
{
    private Trie? _cache;
    private readonly IVoiceCommandCompiler _voiceCommandCompiler;
    private readonly ILogger<TrieFactory> _log;

    public TrieFactory(IVoiceCommandCompiler voiceCommandCompiler, ILogger<TrieFactory> log)
    {
        _voiceCommandCompiler = voiceCommandCompiler;
        _log = log;
    }

    public void Set(PreCompiledVoiceCommand[] commands)
    {
        var root = NodeBuilder.Create();
        root.SetNodeProcessor(new EmptyNodeProcessor());
        _log.ProcessingCommands(new(commands));

        var exceptions = SafeMultipleFunctionExecutor.Execute(commands, root, ProcessCommand);
        if (exceptions.Length != 0)
        {
            throw new AggregateException(exceptions); //TODO We should handle this smarter - failure events
        }

        _cache = new(root.Build());
    }

    private void ProcessCommand(PreCompiledVoiceCommand command, NodeBuilder root)
    {
        using var context = _log.AddContext("Command", command);
        _log.ReadingCommand();
        var nodeProcessors = _voiceCommandCompiler.Compile(command);
        _log.CommandCompiled(new(nodeProcessors));
        var (currentNode, remainingProcessors) = Traverse(root, nodeProcessors, command.TagRequirements);
        BranchOut(currentNode, remainingProcessors, command.TagRequirements);
        _log.TrieState(root);
    }

    public ITrie? Get()
    {
        if (_cache is null)
        {
            _log.CacheNull();
        }

        _log.RetrievingTrie(_cache);
        return _cache;
    }

    private static void BranchOut(NodeBuilder currentNode,
                                  INodeProcessor[] remainingProcessors,
                                  TagRequirement[] tagRequirements)
    {
        foreach (var processor in remainingProcessors)
        {
            var builder = NodeBuilder.Create()
                                     .SetNodeProcessor(processor)
                                     .AddTagRequirements(tagRequirements);

            currentNode.AddChild(builder);
            currentNode = builder;
        }
    }

    private static (NodeBuilder Builder, INodeProcessor[] remainingProcessors) Traverse(
        NodeBuilder builder, INodeProcessor[] nodeProcessors, TagRequirement[] tagRequirements)
    {
        builder.AddTagRequirements(tagRequirements);

        if (nodeProcessors.Length == 0)
        {
            return (builder, Array.Empty<INodeProcessor>());
        }

        var matchingNode = builder.Children.FirstOrDefault(x => nodeProcessors[0].Equals(x.NodeProcessor));
        return matchingNode is not null
                   ? Traverse(matchingNode, nodeProcessors[1..], tagRequirements)
                   : (builder, nodeProcessors);
    }
}

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Trace,
        Message = "Retrieving Trie from storage\r\n{trie}")]
    public static partial void RetrievingTrie(this ILogger logger, JsonLogger<ITrie> trie);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Trie in cache was found to be null, this should not happen at this point!")]
    public static partial void CacheNull(this ILogger logger);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Command processing started, commands to parse are: {commands}")]
    public static partial void ProcessingCommands(this ILogger logger,
                                                  EnumeratorLogger<PreCompiledVoiceCommand> commands);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "Processing new command...")]
    public static partial void ReadingCommand(this ILogger logger);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Debug,
        Message = "Command compilation succeeded, resulted as: {nodeProcessors}")]
    public static partial void CommandCompiled(this ILogger logger, EnumeratorLogger<INodeProcessor> nodeProcessors);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Trace,
        Message = "Trie state checkpoint\r\n{trie}")]
    public static partial void TrieState(this ILogger logger, JsonLogger<NodeBuilder> trie);
}