using DryIoc.Microsoft.DependencyInjection;

using Tests.Helpers;

using Willow.Core;
using Willow.Core.Eventing.Abstractions;
using Willow.Core.Registration.Abstractions;
using Willow.Speech;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Models;
using Willow.Speech.SpeechToText.Eventing.Events;
using Willow.Speech.Tokenization.Tokens;
using Willow.StartUp;

namespace Tests.Speech;

public sealed class VoiceCommandsEndToEnd
{
    private readonly IServiceProvider _serviceProvider;

    public VoiceCommandsEndToEnd()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        WillowStartup.Register([typeof(ICoreAssemblyMarker).Assembly, typeof(ISpeechAssemblyMarker).Assembly], services);
        services.AddSettings();
        _serviceProvider = services.CreateServiceProvider();
        var registrar = _serviceProvider.GetRequiredService<IAssemblyRegistrationEntry>();
        registrar.RegisterAssemblies([
                                         typeof(ICoreAssemblyMarker).Assembly,
                                         typeof(ISpeechAssemblyMarker).Assembly,
                                         GetType().Assembly
                                     ]);
    }

    [Fact]
    public void Sanity()
    {
        var eventDispatcher = _serviceProvider.GetRequiredService<IEventDispatcher>();
        eventDispatcher.Dispatch(new AudioTranscribedEvent("Test mario Repeat Repeat"));
        eventDispatcher.Flush();

        var testVoiceCommand = _serviceProvider.GetRequiredService<TestVoiceCommand>();
        var testVoiceCommandModifer = _serviceProvider.GetRequiredService<TestVoiceCommandModifer>();

        testVoiceCommand.Called.Should().BeTrue();
        testVoiceCommandModifer.CalledAmount.Should().Be(2);
    }
}

public class TestVoiceCommand : IVoiceCommand
{
    public bool Called { get; private set; }
    public string InvocationPhrase => "Test ?[*capture]:hit";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        if (context.Parameters.TryGetValue("capture", out var token)
            && context.Parameters.TryGetValue("hit", out _)
            && token == new WordToken("mario"))
        {
            Called = true;
        }

        return Task.CompletedTask;
    }
}

public class TestVoiceCommandModifer : IVoiceCommand
{
    public int CalledAmount { get; private set; }
    public string InvocationPhrase => "Repeat";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        CalledAmount++;
        return Task.CompletedTask;
    }
}
