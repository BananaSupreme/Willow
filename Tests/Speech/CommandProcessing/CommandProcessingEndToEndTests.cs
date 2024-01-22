using Tests.Helpers;

using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Models;
using Willow.Core.Eventing.Abstractions;
using Willow.Core.Eventing.Registration;
using Willow.Core.Middleware.Registration;
using Willow.Helpers.Extensions;
using Willow.Speech.ScriptingInterface.Eventing.Events;
using Willow.Speech.ScriptingInterface.Models;
using Willow.Speech.SpeechToText.Eventing.Events;
using Willow.Speech.Tokenization.Abstractions;
using Willow.Speech.Tokenization.Middleware;
using Willow.Speech.Tokenization.Registration;
using Willow.Speech.Tokenization.Tokenizers;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Registration;
using Willow.Speech.VoiceCommandParsing.Eventing.Events;
using Willow.Speech.VoiceCommandParsing.Eventing.Handlers;

using Xunit.Abstractions;

namespace Tests.Speech.CommandProcessing;

public sealed class CommandProcessingEndToEndTests : IDisposable
{
    private readonly IEnvironmentStateProvider _environmentStateProvider;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly Fixture _fixture;
    private readonly ITestHandler _handler;
    private readonly ServiceProvider _provider;

    public CommandProcessingEndToEndTests(ITestOutputHelper testOutputHelper)
    {
        _fixture = new Fixture();

        _fixture.Register(creator: static () => new Dictionary<string, object>());
        _fixture.Register(creator: static () => new TagRequirement[] { new(Tags: []) });

        _handler = Substitute.For<ITestHandler>();
        _environmentStateProvider = Substitute.For<IEnvironmentStateProvider>();
        var services = new ServiceCollection();
        services.AddTestLogger(testOutputHelper);
        RegisterServices(services: services);
        services.AddAllTypesFromOwnAssembly<INodeCompiler>(lifetime: ServiceLifetime.Singleton);
        _provider = services.BuildServiceProvider();
        _eventDispatcher = _provider.GetRequiredService<IEventDispatcher>();
        RegisterEvents();
    }

    public void Dispose()
    {
        _provider.Dispose();
    }

    private void RegisterEvents()
    {
        _eventDispatcher.RegisterHandler<AudioTranscribedEvent, AudioTranscribedEventHandler>();
        _eventDispatcher.RegisterHandler<CommandModifiedEvent, CommandModifiedEventHandler>();
        _eventDispatcher.RegisterHandler<CommandParsedEvent, ITestHandler>();
    }

    private void RegisterServices(ServiceCollection services)
    {
        services.AddSingleton(implementationFactory: _ => _environmentStateProvider);
        services.AddSingleton(implementationFactory: _ => _handler);
        services.AddSingleton<AudioTranscribedEventHandler>();
        services.AddSingleton<CommandModifiedEventHandler>();
        services.AddSingleton<PunctuationRemoverMiddleware>();
        services.AddAllTypesFromOwnAssembly<ITranscriptionTokenizer>(ServiceLifetime.Singleton);
        services.AddSettings();
        VoiceCommandCompilationRegistrar.RegisterServices(services: services);
        TokenizationRegistrar.RegisterServices(services: services);
        EventingRegistrar.RegisterServices(services: services);
        MiddlewareRegistrar.RegisterServices(services: services);
    }

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
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go away #input"] }
        ];

        var tokenizer = _provider.GetServices<ITranscriptionTokenizer>()
                                 .OfType<HomophonesTranscriptionTokenizer>()
                                 .First();

        await tokenizer.FlushAsync();

        //Unless the default settings change, gogh is a homophone of go, if this breaks, we need to mock the settings here
        //but I was lazy
        TestInternal("gogh away 42",
                     [],
                     [commands[0].Id],
                     [new Dictionary<string, Token> { { "input", new NumberToken(Value: 42) } }],
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

    private void TestInternal(string input,
                              Tag[] currentEnvironment,
                              Guid[] expectedCommand,
                              Dictionary<string, Token>[] expectedCaptures,
                              RawVoiceCommand[] commands)
    {
        _environmentStateProvider.Tags.Returns(returnThis: currentEnvironment);
        List<Guid> result = [];
        List<Dictionary<string, Token>> captured = [];

        _handler.WhenForAnyArgs(substituteCall: static x => x.HandleAsync(@event: Arg.Any<CommandParsedEvent>()))
                .Do(callbackWithArguments: x =>
                {
                    var @event = (CommandParsedEvent)x[index: 0];
                    result.Add(item: @event.Id);
                    captured.Add(item: @event.Parameters);
                });

        _eventDispatcher.Dispatch(@event: new CommandModifiedEvent(Commands: commands));
        _eventDispatcher.Flush();

        _eventDispatcher.Dispatch(@event: new AudioTranscribedEvent(Text: input));
        _eventDispatcher.Flush();
        _eventDispatcher.Flush(); //Second time because the event can trigger downstream events

        result.Should().BeEquivalentTo(expectation: expectedCommand);
        captured.Should().BeEquivalentTo(expectation: expectedCaptures);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public interface ITestHandler : IEventHandler<CommandParsedEvent>;
}
