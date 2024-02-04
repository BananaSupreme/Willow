using Willow.Speech.ScriptingInterface;
using Willow.Speech.Tokenization.Consts;
using Willow.Speech.VoiceCommandCompilation.Exceptions;
using Willow.Speech.VoiceCommandParsing;

namespace Willow.Speech.VoiceCommandCompilation.Extensions;

internal static partial class SpanExtensions
{
    /// <summary>
    /// Parses the incoming string for a group of processors seperated by a | mark.
    /// </summary>
    /// <param name="processors">The incoming string.</param>
    /// <param name="compilers">All the compilers available in the system.</param>
    /// <param name="capturedValues">The values captured from the <see cref="IVoiceCommand" /> instance.</param>
    /// <returns>The processors that build the compilation.</returns>
    public static INodeProcessor[] ExtractNodeProcessors(this ReadOnlySpan<char> processors,
                                                         INodeCompiler[] compilers,
                                                         IDictionary<string, object> capturedValues)
    {
        var index = 0;
        List<INodeProcessor> nodeProcessors = [];
        while (index > -1)
        {
            index = processors.GetIndexOfNextSeparator();
            if (index == -1)
            {
                processors.ParseNodeProcessor(compilers, capturedValues, nodeProcessors);
            }
            else
            {
                var processor = processors[..index];
                processor.ParseNodeProcessor(compilers, capturedValues, nodeProcessors);
                processors = processors[(index + 1)..];
            }
        }

        return nodeProcessors.ToArray();
    }

    private static void ParseNodeProcessor(this ReadOnlySpan<char> processors,
                                           INodeCompiler[] compilers,
                                           IDictionary<string, object> capturedValues,
                                           List<INodeProcessor> nodeProcessors)
    {
        var nodeProcessor = processors.ParseCommandWord(compilers, capturedValues);
        if (nodeProcessor.IsLeaf)
        {
            throw new CommandCompilationException("Groupings cannot contain leaf nodes.");
        }

        nodeProcessors.Add(nodeProcessor);
    }

    private static int GetIndexOfNextSeparator(this ReadOnlySpan<char> processors)
    {
        var originalLength = processors.Length;
        var nextPipe = processors.IndexOf(Chars.Pipe);
        var nextCapturingGroupOpener = processors.IndexOf(Chars.LeftSquare);
        //If this is true the pipe is potentially coming from inside the capturing group
        //for example "&[[one|of]:capturing|another]" - the next pipe variable will be the pipe inside the OneOf node
        //In this case we want to make sure we close off all the capturing groups before carrying forward.
        if (nextCapturingGroupOpener > -1 && nextPipe > nextCapturingGroupOpener)
        {
            processors = ExitInnerCapturingGroups(processors, nextCapturingGroupOpener);

            //This means its missing a separator "&[..|~[..]~[..]]"
            //Remember that we get here without the outer squares so above is ..|~[..]~[..]
            //We should either be at the end, have a separator or we can be looking at a capture name such as ..|~[..]:..
            nextPipe = processors.IndexOf(Chars.Pipe);
            nextCapturingGroupOpener = processors.IndexOf(Chars.LeftSquare);
            if (processors.Length != 0 && nextCapturingGroupOpener > -1 && nextPipe > nextCapturingGroupOpener)
            {
                throw new CommandCompilationException("missing a separator between capturing groups");
            }
        }

        //If we had an inner group we shaved off some characters, so next pipe is not at 0,
        //its at 0 plus how many characters we shaved
        //of course if we are at the end of the line we should be sure to return -1 to signal that there are no more pipes
        return nextPipe > 0 ? nextPipe + (originalLength - processors.Length) : -1;
    }

    private static ReadOnlySpan<char> ExitInnerCapturingGroups(this ReadOnlySpan<char> processors,
                                                               int nextCapturingGroupOpener)
    {
        var numberOfInnerCaptures = 1;
        processors = processors[nextCapturingGroupOpener..];
        //Those capturing groups can also be deeply nested, this is probably bad practice,
        //but we should still support it
        //for example "&[~[...]]"
        while (numberOfInnerCaptures > 0)
        {
            var nextCapturingGroupCloser = processors.IndexOf(Chars.RightSquare);
            if (nextCapturingGroupCloser < 0)
            {
                throw new CommandCompilationException("Expected the group to close, missing ']'");
            }

            numberOfInnerCaptures += processors[1..nextCapturingGroupCloser].Count(Chars.LeftSquare) - 1;
            processors = processors[(nextCapturingGroupCloser + 1)..];
        }

        return processors;
    }
}
