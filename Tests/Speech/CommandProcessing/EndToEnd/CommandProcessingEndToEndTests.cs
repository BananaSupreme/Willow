using Willow.Environment.Models;
using Willow.Speech.ScriptingInterface.Models;
using Willow.Speech.Tokenization;
using Willow.Speech.Tokenization.Tokenizers;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Tests.Speech.CommandProcessing.EndToEnd;

public sealed partial class CommandProcessingEndToEndTests
{
    [Fact]
    public void When_MatchingCommandsWithSingleRequirement_CommandIsProcessedCorrectly()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with
            {
                InvocationPhrases = ["go"],
                TagRequirements = [new TagRequirement(Tags: [new Tag(Name: "Requirement")])]
            },
            _fixture.Create<RawVoiceCommand>() with
            {
                InvocationPhrases = ["go"],
                TagRequirements = [new TagRequirement(Tags: [new Tag(Name: "Other")])]
            }
        ];

        TestInternal("go", [new Tag(Name: "Requirement")], [commands[0].Id], [[]], commands);
    }

    [Fact]
    public void When_MatchingCommandsWithMultipleSpecificRequirements_MoreSpecificCommandIsPrioritized()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with
            {
                InvocationPhrases = ["[one|two|go]:enter away *now"],
                TagRequirements = [new TagRequirement(Tags: [new Tag(Name: "Requirement")])]
            },
            _fixture.Create<RawVoiceCommand>() with
            {
                InvocationPhrases = ["[one|two|go]:enter away *now"],
                TagRequirements
                = [new TagRequirement(Tags: [new Tag(Name: "Requirement"), new Tag(Name: "Other")])]
            },
            _fixture.Create<RawVoiceCommand>() with
            {
                InvocationPhrases = ["[one|two|go]:enter away *now"],
                TagRequirements = [new TagRequirement(Tags: [new Tag(Name: "Other")])]
            }
        ];

        TestInternal("go away now",
                     [new Tag(Name: "Requirement"), new Tag(Name: "Other")],
                     [commands[1].Id],
                     [
                         new Dictionary<string, Token>
                         {
                             { "enter", new WordToken(Value: "go") }, { "now", new WordToken(Value: "now") }
                         }
                     ],
                     commands);
    }

    [Fact]
    public void When_ProcessingEmptyRequirement_AlwaysSucceeds()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with
            {
                InvocationPhrases = ["go ?[#away]:hit **now"],
                TagRequirements = [new TagRequirement(Tags: [new Tag(Name: "Three")])]
            },
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go ?[#away]:hit **now"] }
        ];

        TestInternal("go now",
                     [new Tag(Name: "One"), new Tag(Name: "Two")],
                     [commands[1].Id],
                     [new Dictionary<string, Token> { { "now", new WordToken(Value: "now") } }],
                     commands);
    }

    [Fact]
    public void When_CommandMatchesWithOneAdditionalWord_MatchesSuccessfully()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go away #input"] }
        ];

        TestInternal("go away 42 now",
                     [],
                     [commands[0].Id],
                     [new Dictionary<string, Token> { { "input", new NumberToken(Value: 42) } }],
                     commands);
    }

    [Fact]
    public void When_CommandMatchesWithOneLessWord_DoesNotMatch()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go away ?[*word]:hit"] }
        ];

        TestInternal("go", [], [], [], commands);
    }

    [Fact]
    public void When_ProcessingSwallowedExpressions_Matches()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go ?[*away]:captured now"] },
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go ?[*away]:captured"] }
        ];

        TestInternal("go away elsewhere",
                     [],
                     [commands[1].Id],
                     [
                         new Dictionary<string, Token>
                         {
                             { "away", new WordToken(Value: "away") }, { "captured", new EmptyToken() }
                         }
                     ],
                     commands);
    }

    [Fact]
    public void When_ProcessingCommandInChangingEnvironment_CorrectlyAdaptsToNewConditions()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with
            {
                InvocationPhrases = ["go away ?[now]:_"],
                TagRequirements = [new TagRequirement(Tags: [new Tag(Name: "First")])]
            },
            _fixture.Create<RawVoiceCommand>() with
            {
                InvocationPhrases = ["go away"],
                TagRequirements = [new TagRequirement(Tags: [new Tag(Name: "Second")])]
            }
        ];

        TestInternal("go away now", [new Tag(Name: "Second")], [commands[1].Id], [[]], commands);

        TestInternal("go away now",
                     [new Tag(Name: "First")],
                     [commands[0].Id],
                     [new Dictionary<string, Token> { { "_", new EmptyToken() } }],
                     commands);
    }

    [Fact]
    public void When_TranscriptionIsPunctuated_PunctuationIsIgnored()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go away #input"] }
        ];

        TestInternal("go! ~away, 42$",
                     [],
                     [commands[0].Id],
                     [new Dictionary<string, Token> { { "input", new NumberToken(Value: 42) } }],
                     commands);
    }

    [Fact]
    public async Task When_MatchByHomophones_MatchCorrect()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go [coarse|hello]:hit #input"] }
        ];

        var tokenizer = _provider.GetServices<ITranscriptionTokenizer>()
                                 .OfType<HomophonesTranscriptionTokenizer>()
                                 .First();

        await tokenizer.FlushAsync();

        TestInternal("gogh course 42",
                     [],
                     [commands[0].Id],
                     [
                         new Dictionary<string, Token>
                         {
                             { "input", new NumberToken(Value: 42) }, { "hit", new WordToken("coarse") }
                         }
                     ],
                     commands);
    }

    [Fact]
    public void When_CapitalizationIsMismatched_StillMatches()
    {
        RawVoiceCommand[] commands = [_fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go away NOW"] }];

        TestInternal("Go AwAy now", [], [commands[0].Id], [[]], commands);
    }

    [Fact]
    public void When_CallingMultipleCommandsInSuccession_AllGetCalled()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go away #input"] },
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["after you"] }
        ];

        TestInternal("go away 42, after you!",
                     [],
                     [commands[0].Id, commands[1].Id],
                     [new Dictionary<string, Token> { { "input", new NumberToken(Value: 42) } }, []],
                     commands);
    }
}
