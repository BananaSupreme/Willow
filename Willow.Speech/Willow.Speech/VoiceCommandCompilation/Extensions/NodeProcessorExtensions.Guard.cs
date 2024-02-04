using Willow.Speech.VoiceCommandCompilation.Exceptions;
using Willow.Speech.VoiceCommandParsing;

namespace Willow.Speech.VoiceCommandCompilation.Extensions;

internal static class NodeProcessorExtensions
{
    /// <summary>
    /// Guards that <paramref name="node" /> is not of type <typeparamref name="T" />.
    /// </summary>
    /// <param name="node">The node to test.</param>
    /// <typeparam name="T">The type that <paramref name="node" /> should not be.</typeparam>
    /// <exception cref="CommandCompilationException">Throws when they are the same.</exception>
    public static void GuardNotSame<T>(this INodeProcessor node) where T : INodeProcessor
    {
        if (node.GetType() == typeof(T))
        {
            throw new CommandCompilationException("Inner node cannot be same as outer node");
        }
    }
}
