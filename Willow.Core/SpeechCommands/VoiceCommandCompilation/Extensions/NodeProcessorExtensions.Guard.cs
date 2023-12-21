using Willow.Core.SpeechCommands.VoiceCommandCompilation.Exceptions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;

namespace Willow.Core.SpeechCommands.VoiceCommandCompilation.Extensions;

internal static class NodeProcessorExtensions
{
    public static void GuardNotSame<T>(this INodeProcessor node)
        where T : INodeProcessor
    {
        if (node.GetType() == typeof(T))
        {
            throw new CommandCompilationException("Inner node cannot be same as outer node");
        }
    }
}