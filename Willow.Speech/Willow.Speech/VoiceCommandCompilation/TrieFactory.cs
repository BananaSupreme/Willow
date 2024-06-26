﻿using Willow.Environment.Models;
using Willow.Helpers;
using Willow.Helpers.Logging.Extensions;
using Willow.Helpers.Logging.Loggers;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Models;
using Willow.Speech.VoiceCommandParsing;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandCompilation;

internal sealed class TrieFactory : ITrieFactory
{
    private readonly ILogger<TrieFactory> _log;
    private readonly IVoiceCommandCompiler _voiceCommandCompiler;
    private Trie? _cache;

    public TrieFactory(IVoiceCommandCompiler voiceCommandCompiler, ILogger<TrieFactory> log)
    {
        _voiceCommandCompiler = voiceCommandCompiler;
        _log = log;
    }

    public void Set(PreCompiledVoiceCommand[] commands)
    {
        var root = NodeBuilder.Create();
        root.SetNodeProcessor(new EmptyNodeProcessor());
        _log.ProcessingCommands(new EnumeratorLogger<PreCompiledVoiceCommand>(commands));

        _ = SafeMultipleFunctionExecutor.Execute(commands, root, ProcessCommand, onException: LogException);

        _cache = new Trie(root.Build());
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

    private void ProcessCommand(PreCompiledVoiceCommand command, NodeBuilder root)
    {
        using var context = _log.AddContext("Command", command);
        _log.ReadingCommand();
        var nodeProcessors = _voiceCommandCompiler.Compile(command);
        _log.CommandCompiled(new EnumeratorLogger<INodeProcessor>(nodeProcessors));
        var (currentNode, remainingProcessors) = Traverse(root, nodeProcessors, command.TagRequirements);
        BranchOut(currentNode, remainingProcessors, command.TagRequirements);
        _log.TrieState(root);
    }

    private void LogException(PreCompiledVoiceCommand command, NodeBuilder _, Exception ex)
    {
        _log.CompilationFailed(command, ex);
    }

    private static void BranchOut(NodeBuilder currentNode,
                                  INodeProcessor[] remainingProcessors,
                                  TagRequirement[] tagRequirements)
    {
        foreach (var processor in remainingProcessors)
        {
            var builder = NodeBuilder.Create().SetNodeProcessor(processor).AddTagRequirements(tagRequirements);

            currentNode.AddChild(builder);
            currentNode = builder;
        }
    }

    private static (NodeBuilder Builder, INodeProcessor[] remainingProcessors) Traverse(NodeBuilder builder,
        INodeProcessor[] nodeProcessors,
        TagRequirement[] tagRequirements)
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

internal static partial class TrieFactoryLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Trace, Message = "Retrieving Trie from storage\r\n{trie}")]
    public static partial void RetrievingTrie(this ILogger logger, JsonLogger<Trie> trie);

    [LoggerMessage(EventId = 2,
                   Level = LogLevel.Error,
                   Message = "Trie in cache was found to be null, this should not happen at this point!")]
    public static partial void CacheNull(this ILogger logger);

    [LoggerMessage(EventId = 3,
                   Level = LogLevel.Information,
                   Message = "Command processing started, commands to parse are: {commands}")]
    public static partial void ProcessingCommands(this ILogger logger,
                                                  EnumeratorLogger<PreCompiledVoiceCommand> commands);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Processing new command...")]
    public static partial void ReadingCommand(this ILogger logger);

    [LoggerMessage(EventId = 5,
                   Level = LogLevel.Debug,
                   Message = "Command compilation succeeded, resulted as: {nodeProcessors}")]
    public static partial void CommandCompiled(this ILogger logger, EnumeratorLogger<INodeProcessor> nodeProcessors);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Compilation of ({command}) failed!")]
    public static partial void CompilationFailed(this ILogger logger, PreCompiledVoiceCommand command, Exception ex);

    [LoggerMessage(EventId = 7, Level = LogLevel.Trace, Message = "Trie state checkpoint\r\n{trie}")]
    public static partial void TrieState(this ILogger logger, JsonLogger<NodeBuilder> trie);
}
