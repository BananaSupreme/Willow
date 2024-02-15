using Tests.Helpers;

using Willow.Environment;
using Willow.Environment.Models;
using Willow.Eventing;
using Willow.Eventing.Registration;
using Willow.Helpers.Extensions;
using Willow.Middleware.Registration;
using Willow.Registration;
using Willow.Settings;
using Willow.Speech.ScriptingInterface.Eventing.Events;
using Willow.Speech.ScriptingInterface.Models;
using Willow.Speech.SpeechToText.Events;
using Willow.Speech.Tokenization;
using Willow.Speech.Tokenization.Enums;
using Willow.Speech.Tokenization.Middleware;
using Willow.Speech.Tokenization.Registration;
using Willow.Speech.Tokenization.Settings;
using Willow.Speech.Tokenization.Tokenizers;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandCompilation;
using Willow.Speech.VoiceCommandCompilation.NodeCompilers;
using Willow.Speech.VoiceCommandCompilation.Registration;
using Willow.Speech.VoiceCommandParsing.EventHandlers;
using Willow.Speech.VoiceCommandParsing.Events;

using Xunit.Abstractions;

namespace Tests.Speech.CommandProcessing.EndToEnd;

public sealed partial class CommandProcessingEndToEndTests : IDisposable
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
        _eventDispatcher.RegisterHandler<CommandsAddedEvent, CommandModifiedEventHandler>();
        _eventDispatcher.RegisterHandler<CommandParsedEvent, ITestHandler>();
    }

    private void RegisterServices(ServiceCollection services)
    {
        var homophoneSettings = Substitute.For<ISettings<HomophoneSettings>>();
        homophoneSettings.CurrentValue.Returns(
            new HomophoneSettings(true, HomophoneType.CarnegieMelonDictionaryEquivalents, []));
        services.AddRegistration();
        services.AddSettings();
        services.AddSingleton(implementationFactory: _ => _environmentStateProvider);
        services.AddSingleton(implementationFactory: _ => _handler);
        services.AddSingleton<ISettings<HomophoneSettings>>(_ => homophoneSettings);
        services.AddSingleton<AudioTranscribedEventHandler>();
        services.AddSingleton<CommandModifiedEventHandler>();
        services.AddSingleton<PunctuationRemoverMiddleware>();
        services.AddAllTypesFromAssemblyMarked<ITranscriptionTokenizer, HomophonesTranscriptionTokenizer>();
        services.AddAllTypesFromAssemblyMarked<INodeCompiler, AndNodeCompiler>();
        new VoiceCommandCompilationRegistrar().RegisterServices(services: services);
        new TokenizationRegistrar().RegisterServices(services: services);
        new EventingRegistrar().RegisterServices(services: services);
        new MiddlewareRegistrar().RegisterServices(services: services);
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

        _eventDispatcher.Dispatch(@event: new CommandsAddedEvent(Commands: commands));
        _eventDispatcher.Flush();

        _eventDispatcher.Dispatch(@event: new AudioTranscribedEvent(Text: input));
        _eventDispatcher.Flush();

        result.Should().BeEquivalentTo(expectation: expectedCommand);
        captured.Should().BeEquivalentTo(expectation: expectedCaptures);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public interface ITestHandler : IEventHandler<CommandParsedEvent>;
}
