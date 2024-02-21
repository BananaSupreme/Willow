using Tests.Helpers;

using Willow;
using Willow.Eventing;
using Willow.Registration;
using Willow.Settings;
using Willow.Speech;
using Willow.Speech.Microphone.Settings;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;
using Willow.Speech.SpeechToText.Events;
using Willow.Speech.Tokenization.Tokens;

using Xunit.Abstractions;

namespace Tests.Speech;

public sealed class VoiceCommandsEndToEnd : IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    public VoiceCommandsEndToEnd(ITestOutputHelper testOutputHelper)
    {
        _serviceProvider = WillowStartup.StartAsync(null,
                                                    s =>
                                                    {
                                                        s.AddTestLogger(testOutputHelper);
                                                        s.AddSettings();
                                                        s.AddSingleton<Counter>();
                                                        return s;
                                                    })
                                        .GetAwaiter()
                                        .GetResult();
        var registrar = _serviceProvider.GetRequiredService<IAssemblyRegistrationEntry>();
        TurnMicrophoneOff();
        registrar.RegisterAssembliesAsync([typeof(ISpeechAssemblyMarker).Assembly, GetType().Assembly])
                 .GetAwaiter()
                 .GetResult();
    }

    //This just breaks tests, microphone being on doesn't really mean anything in a test environment.
    private void TurnMicrophoneOff()
    {
        var settings = _serviceProvider.GetRequiredService<ISettings<MicrophoneSettings>>();
        settings.Update(settings.CurrentValue with { ShouldRecordAudio = false });
        (settings as IDisposable)?.Dispose();
    }

    [Fact]
    public void Sanity()
    {
        var eventDispatcher = _serviceProvider.GetRequiredService<IEventDispatcher>();
        eventDispatcher.Flush();
        eventDispatcher.Dispatch(new AudioTranscribedEvent("Test mario Repeat Repeat"));
        eventDispatcher.Flush();

        var testVoiceCommand = _serviceProvider.GetRequiredService<TestVoiceCommand>();
        var counter = _serviceProvider.GetRequiredService<Counter>();

        testVoiceCommand.Called.Should().BeTrue();
        counter.Counted.Should().Be(2);
    }

    public void Dispose()
    {
        (_serviceProvider as IDisposable)?.Dispose();
        Directory.Delete(ISettings<MicrophoneSettings>.SettingsFolderPath, true);
    }
}

public class TestVoiceCommand : IVoiceCommand
{
    private readonly Counter _counter;
    public bool Called { get; private set; }
    public string InvocationPhrase => "Test ?[*capture]:hit";

    public TestVoiceCommand(Counter counter)
    {
        _counter = counter;
    }

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        if (context.Parameters.TryGetValue("capture", out var token)
            && context.Parameters.TryGetValue("hit", out _)
            && token.Match(new WordToken("mario")))
        {
            Called = true;
        }

        return Task.CompletedTask;
    }

    [VoiceCommand("Repeat")]
    public Task TestVoiceCommandModifer(VoiceCommandContext context)
    {
        //This indirection is because the source generator copies the properties to another class.
        _counter.Counted++;
        return Task.CompletedTask;
    }
}

public sealed class Counter
{
    public int Counted { get; set; }
}
