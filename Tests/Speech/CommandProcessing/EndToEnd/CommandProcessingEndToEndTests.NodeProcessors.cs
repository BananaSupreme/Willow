using Willow.Speech.ScriptingInterface.Models;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Tests.Speech.CommandProcessing.EndToEnd;

public sealed partial class CommandProcessingEndToEndTests
{
    [Fact]
    public void When_ProcessingRepeatingWildCardOrWildCardCommandWithNoWords_FailsAsExpected()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go **phrase"] },
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["[one|two|go]:enter *phrase"] }
        ];

        TestInternal("go", [], [], [], commands);
    }

    [Fact]
    public void When_OptionalCommandCaptures_AddsFlag()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go ?[phrase]:found"] }
        ];

        TestInternal("go phrase",
                     [],
                     [commands[0].Id],
                     [new Dictionary<string, Token> { { "found", new EmptyToken() } }],
                     commands);
    }

    [Fact]
    public void When_OneOfCommandWithMultipleWords_Captures()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go [phrase|hello world]:found"] }
        ];

        TestInternal("go hello world",
                     [],
                     [commands[0].Id],
                     [
                         new Dictionary<string, Token>
                         {
                             { "found", new MergedToken([new WordToken("hello"), new WordToken("world")]) }
                         }
                     ],
                     commands);
    }
}
