﻿using Willow.Speech.Tokenization.Consts;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Exceptions;
using Willow.Speech.VoiceCommandCompilation.Extensions;
using Willow.Speech.VoiceCommandParsing;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Willow.Speech.VoiceCommandCompilation.NodeCompilers;

/// <summary>
/// Compiles the patterns
/// <code>
/// [capturedVariableName]:variableName
/// [..|..]:variableName
/// </code>
/// Same as <see cref="OneOfNodeCompiler" /> but the capturing comes at the square brackets in the beginning instead.
/// </summary>
/// <seealso cref="OneOfNodeCompiler" />
internal sealed class OneOfSpecialSyntaxNodeCompiler : INodeCompiler
{
    private static readonly char[] _startSymbols = "[".ToCharArray();

    public (bool IsSuccefful, INodeProcessor ProccessedNode) TryParse(ReadOnlySpan<char> commandWord,
                                                                      IDictionary<string, object> capturedValues,
                                                                      INodeCompiler[] compilers)
    {
        if (!commandWord.StartsWith(_startSymbols))
        {
            return INodeCompiler.Fail();
        }

        var separatorIndex = commandWord.IndexOf(Chars.Colon);
        var tokens = GetTokens(commandWord[..separatorIndex], capturedValues);

        commandWord = commandWord[(separatorIndex + 1)..];
        var captureValue = commandWord.GuardValidVariableName();

        return (true, new OneOfNodeProcessor(captureValue.ToString(), tokens));
    }

    private static Token[] GetTokens(ReadOnlySpan<char> variables, IDictionary<string, object> capturedValues)
    {
        variables = variables.GuardWrappedInSquares();
        if (variables.IsEmpty)
        {
            throw new CommandCompilationException(
                "OneOf must have a variable, either a reference to a captured list or a list syntax");
        }

        return variables[0] == Chars.Underscore
                   ? variables.GetTokensFromCaptured(capturedValues)
                   : variables.ExtractFromInLineList();
    }
}
