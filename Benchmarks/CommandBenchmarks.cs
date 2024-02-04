using BenchmarkDotNet.Attributes;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Willow;
using Willow.BuiltInCommands;
using Willow.DeviceAutomation.InputDevices;
using Willow.Eventing;
using Willow.Registration;
using Willow.Speech;
using Willow.Speech.SpeechToText.Events;
using Willow.Speech.Tokenization;
using Willow.Speech.Tokenization.Tokenizers;

// ReSharper disable ClassCanBeSealed.Global

namespace Benchmarks;

[MemoryDiagnoser]
public class CommandBenchmarks
{
    private IEventDispatcher _dispatcher = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(static _ => Substitute.For<IInputSimulator>());
        var provider = await WillowStartup.StartAsync(services);
        _dispatcher = provider.GetRequiredService<IEventDispatcher>();
        var registrar = provider.GetRequiredService<IAssemblyRegistrationEntry>();
        await registrar.RegisterAssembliesAsync([
                                                    typeof(ICoreAssemblyMarker).Assembly,
                                                    typeof(ISpeechAssemblyMarker).Assembly,
                                                    typeof(IBuiltInCommandsAssemblyMarker).Assembly
                                                ]);

        var tokenizer = provider.GetServices<ITranscriptionTokenizer>()
                                .OfType<HomophonesTranscriptionTokenizer>()
                                .First();
        tokenizer.FlushAsync().GetAwaiter().GetResult();
    }

    [Benchmark]
    public void MoveMouseUp()
    {
        _dispatcher.Dispatch(
            new AudioTranscribedEvent("move mouse up 72 move mouse down 72 move mouse left 72 move mouse right 72"));
        _dispatcher.Flush();
    }

    public static async Task Test()
    {
        var container = new CommandBenchmarks();
        await container.Setup();
        container.MoveMouseUp();
    }
}
