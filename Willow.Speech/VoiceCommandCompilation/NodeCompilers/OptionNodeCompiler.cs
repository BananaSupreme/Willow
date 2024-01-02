﻿using Willow.Speech.Tokenization.Consts;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Exceptions;
using Willow.Speech.VoiceCommandCompilation.Extensions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandCompilation.NodeCompilers;

internal sealed class OptionNodeCompiler : INodeCompiler
{
    private static readonly char[][] _startSymbols = ["Optional".ToCharArray(), "Opt".ToCharArray(), "?".ToCharArray()];

    public (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                                      IDictionary<string, object> capturedValues,
                                                                      INodeCompiler[] compilers)
    {
        var startSymbol = commandWord.FirstToStartWithOrNull(_startSymbols);
        if (startSymbol is null)
        {
            return INodeCompiler.Fail();
        }

        var separatorIndex = commandWord.LastIndexOf(Chars.Colon);
        if (separatorIndex < 0)
        {
            throw new CommandCompilationException("Expected a flag name, but found none");
        }

        var captureValue = commandWord[startSymbol.Length..separatorIndex].GuardWrappedInSquares();
        var node = captureValue.ParseCommandWord(compilers, capturedValues);
        node.GuardNotSame<OptionalNodeProcessor>();

        var flagName = commandWord[(separatorIndex + 1)..];
        flagName = flagName.GuardValidVariableName();

        return (true, new OptionalNodeProcessor(flagName.ToString(), node));
    }
}