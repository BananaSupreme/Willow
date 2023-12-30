using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Models;
using Willow.Core.Eventing.Abstractions;
using Willow.Core.Eventing.Registration;
using Willow.Core.Helpers.Extensions;
using Willow.Speech.ScriptingInterface.Events;
using Willow.Speech.ScriptingInterface.Models;
using Willow.Speech.SpeechRecognition.SpeechToText.Eventing.Events;
using Willow.Speech.SpeechRecognition.SpeechToText.Registration;
using Willow.Speech.Tokenization.Eventing.Interceptors;
using Willow.Speech.Tokenization.Registration;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Registration;
using Willow.Speech.VoiceCommandParsing.Eventing.Events;
using Willow.Speech.VoiceCommandParsing.Eventing.Handlers;

namespace Tests.VoiceCommands.CommandProcessing;

public class CommandProcessingEndToEndTests
{
    private readonly Fixture _fixture;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IEnvironmentStateProvider _environmentStateProvider;
    private readonly ITestHandler _handler;

    public CommandProcessingEndToEndTests()
    {
        _fixture = new();

        _fixture.Register(() => new Dictionary<string, object>());
        _fixture.Register(() => new TagRequirement[] { new([]) });

        _handler = Substitute.For<ITestHandler>();
        _environmentStateProvider = Substitute.For<IEnvironmentStateProvider>();
        var services = new ServiceCollection();
        RegisterServices(services);
        services.AddAllTypesFromOwnAssembly<INodeCompiler>(ServiceLifetime.Singleton);
        var serviceProvider = services.BuildServiceProvider();
        _eventDispatcher = serviceProvider.GetRequiredService<IEventDispatcher>();
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        AudioTranscribedEventInterceptorRegistrar.RegisterInterceptor(eventDispatcher: _eventDispatcher);
        _eventDispatcher.RegisterHandler<AudioTranscribedEvent, AudioTranscribedEventHandler>();
        _eventDispatcher.RegisterHandler<CommandModifiedEvent, CommandModifiedEventHandler>();
        _eventDispatcher.RegisterHandler<CommandParsedEvent, ITestHandler>();
    }

    private void RegisterServices(ServiceCollection services)
    {
        services.AddLogging();
        services.AddSingleton(implementationFactory: _ => _environmentStateProvider);
        services.AddSingleton(implementationFactory: _ => _handler);
        services.AddSingleton<AudioTranscribedEventHandler>();
        services.AddSingleton<CommandModifiedEventHandler>();
        services.AddSingleton<PunctuationRemoverInterceptor>();

        VoiceCommandCompilationRegistrar.RegisterServices(services: services);
        TokenizationRegistrar.RegisterServices(services: services);
        EventingServiceRegistrar.RegisterServices(services: services);
    }

    [Fact]
    public async Task When_MatchingCommandsWithSingleRequirement_CommandIsProcessedCorrectly()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with
            {
                InvocationPhrases = ["go"], TagRequirements = [new(Tags: [new(Name: "Requirement")])]
            },
            _fixture.Create<RawVoiceCommand>() with
            {
                InvocationPhrases = ["go"], TagRequirements = [new(Tags: [new(Name: "Other")])]
            }
        ];

        await TestInternal(
            input: "go",
            currentEnvironment: [new(Name: "Requirement")],
            expectedCommand: [commands[0].Id],
            expectedCaptures: [[]],
            commands: commands
        );
    }

    [Fact]
    public async Task When_MatchingCommandsWithMultipleSpecificRequirements_MoreSpecificCommandIsPrioritized()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with
            {
                InvocationPhrases =
                ["[one|two|go]:enter away *now"],
                TagRequirements = [new(Tags: [new(Name: "Requirement")])]
            },
            _fixture.Create<RawVoiceCommand>() with
            {
                InvocationPhrases =
                ["[one|two|go]:enter away *now"],
                TagRequirements = [new(Tags: [new(Name: "Requirement"), new(Name: "Other")])]
            },
            _fixture.Create<RawVoiceCommand>() with
            {
                InvocationPhrases =
                ["[one|two|go]:enter away *now"],
                TagRequirements = [new(Tags: [new(Name: "Other")])]
            }
        ];

        await TestInternal(
            input: "go away now",
            currentEnvironment: [new(Name: "Requirement"), new(Name: "Other")],
            expectedCommand: [commands[1].Id],
            expectedCaptures:
            [new() { { "enter", new WordToken(Value: "go") }, { "now", new WordToken(Value: "now") } }],
            commands: commands
        );
    }

    [Fact]
    public async Task When_ProcessingRepeatingWildCardOrWildCardCommandWithNoWords_FailsAsExpected()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go **phrase"] },
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["[one|two|go]:enter *phrase"] }
        ];

        await TestInternal(
            input: "go",
            currentEnvironment: [],
            expectedCommand: [],
            expectedCaptures: [],
            commands: commands
        );
    }
    
    [Fact]
    public async Task When_OptionalCommandCaptures_AddsFlag()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go ?[phrase]:found"] },
        ];

        await TestInternal(
            input: "go phrase",
            currentEnvironment: [],
            expectedCommand: [commands[0].Id],
            expectedCaptures: [new() { { "found", new EmptyToken() } }],
            commands: commands
        );
    }

    [Fact]
    public async Task When_ProcessingEmptyRequirement_AlwaysSucceeds()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with
            {
                InvocationPhrases = ["go ?[#away]:hit **now"], TagRequirements = [new(Tags: [new(Name: "Three")])]
            },
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go ?[#away]:hit **now"] }
        ];

        await TestInternal(
            input: "go now",
            currentEnvironment: [new(Name: "One"), new(Name: "Two")],
            expectedCommand: [commands[1].Id],
            expectedCaptures: [new() { { "now", new WordToken(Value: "now") } }],
            commands: commands
        );
    }

    [Fact]
    public async Task When_CommandMatchesWithOneAdditionalWord_MatchesSuccessfully()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go away #input"] }
        ];

        await TestInternal(
            input: "go away 42 now",
            currentEnvironment: [],
            expectedCommand: [commands[0].Id],
            expectedCaptures: [new() { { "input", new NumberToken(Value: 42) } }],
            commands: commands
        );
    }

    [Fact]
    public async Task When_CommandMatchesWithOneLessWord_DoesNotMatch()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go away ?[*word]:hit"] }
        ];

        await TestInternal(
            input: "go",
            currentEnvironment: [],
            expectedCommand: [],
            expectedCaptures: [],
            commands: commands
        );
    }

    [Fact]
    public async Task When_ProcessingSwallowedExpressions_Matches()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go ?[*away]:captured now"] },
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go ?[*away]:captured"] }
        ];

        await TestInternal(
            input: "go away elsewhere",
            currentEnvironment: [],
            expectedCommand: [commands[1].Id],
            expectedCaptures: [new() { { "away", new WordToken(Value: "away") }, { "captured", new EmptyToken() } }],
            commands: commands
        );
    }

    [Fact]
    public async Task When_ProcessingCommandInChangingEnvironment_CorrectlyAdaptsToNewConditions()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with
            {
                InvocationPhrases = ["go away ?[now]:_"], TagRequirements = [new(Tags: [new(Name: "First")])]
            },
            _fixture.Create<RawVoiceCommand>() with
            {
                InvocationPhrases = ["go away"], TagRequirements = [new(Tags: [new(Name: "Second")])]
            }
        ];

        await TestInternal(
            input: "go away now",
            currentEnvironment: [new(Name: "Second")],
            expectedCommand: [commands[1].Id],
            expectedCaptures: [[]],
            commands: commands
        );

        await TestInternal(
            input: "go away now",
            currentEnvironment: [new(Name: "First")],
            expectedCommand: [commands[0].Id],
            expectedCaptures: [new() { { "_", new EmptyToken() } }],
            commands: commands
        );
    }

    [Fact]
    public async Task When_TranscriptionIsPunctuated_PunctuationIsIgnored()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go away #input"] }
        ];

        await TestInternal(
            input: "go! ~away, 42$",
            currentEnvironment: [],
            expectedCommand: [commands[0].Id],
            expectedCaptures: [new() { { "input", new NumberToken(Value: 42) } }],
            commands: commands
        );
    }
    
    [Fact]
    public async Task When_CapitalizationIsMismatched_StillMatches()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go away NOW"] }
        ];

        await TestInternal(
            input: "Go AwAy now",
            currentEnvironment: [],
            expectedCommand: [commands[0].Id],
            expectedCaptures: [[]],
            commands: commands
        );
    }

    [Fact]
    public async Task When_CallingMultipleCommandsInSuccession_AllGetCalled()
    {
        RawVoiceCommand[] commands =
        [
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["go away #input"] },
            _fixture.Create<RawVoiceCommand>() with { InvocationPhrases = ["after you"] }
        ];

        await TestInternal(
            input: "go away 42, after you!",
            currentEnvironment: [],
            expectedCommand: [commands[0].Id, commands[1].Id],
            expectedCaptures: [new() { { "input", new NumberToken(Value: 42) } }, []],
            commands: commands
        );
    }

    private async Task TestInternal(string input, Tag[] currentEnvironment, Guid[] expectedCommand,
                                    Dictionary<string, Token>[] expectedCaptures, RawVoiceCommand[] commands)
    {
        _environmentStateProvider.Tags.Returns(returnThis: currentEnvironment);
        List<Guid> result = [];
        List<Dictionary<string, Token>> captured = [];

        _handler.WhenForAnyArgs(substituteCall: x => x.HandleAsync(@event: Arg.Any<CommandParsedEvent>()))
                .Do(callbackWithArguments: x =>
                {
                    var @event = (CommandParsedEvent)x[index: 0];
                    result.Add(item: @event.Id);
                    captured.Add(item: @event.Parameters);
                });

        _eventDispatcher.Dispatch(@event: new CommandModifiedEvent(Commands: commands));
        await _eventDispatcher.FlushAsync();

        _eventDispatcher.Dispatch(@event: new AudioTranscribedEvent(Text: input));
        await _eventDispatcher.FlushAsync();
        await _eventDispatcher.FlushAsync(); //Second time because the event can trigger downstream events

        result.Should().BeEquivalentTo(expectation: expectedCommand);
        captured.Should().BeEquivalentTo(expectation: expectedCaptures);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public interface ITestHandler : IEventHandler<CommandParsedEvent>;
}