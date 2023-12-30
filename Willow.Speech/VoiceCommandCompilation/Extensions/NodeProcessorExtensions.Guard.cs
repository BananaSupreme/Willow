using Willow.Speech.VoiceCommandCompilation.Exceptions;
using Willow.Speech.VoiceCommandParsing.Abstractions;

namespace Willow.Speech.VoiceCommandCompilation.Extensions;

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