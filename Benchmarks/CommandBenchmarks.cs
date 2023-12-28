using BenchmarkDotNet.Attributes;

using DryIoc.Microsoft.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Willow.BuiltInCommands;
using Willow.Core;
using Willow.Core.Eventing.Abstractions;
using Willow.Core.Registration.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.SpeechToText.Eventing.Events;
using Willow.DeviceAutomation;
using Willow.DeviceAutomation.InputDevices.Abstractions;

namespace Benchmarks;

[MemoryDiagnoser]
public class CommandBenchmarks
{
    private IEventDispatcher _dispatcher = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        WillowStartup.Register(services, new ConfigurationManager());
        services.AddSingleton(_ => Substitute.For<IInputSimulator>());
        DeviceAutomationRegistrator.RegisterServices(services);
        var provider = services.CreateServiceProvider();
        _dispatcher = provider.GetRequiredService<IEventDispatcher>();
        var registrar = provider.GetRequiredService<IAssemblyRegistrationEntry>();
        registrar.RegisterAssemblies([typeof(WillowStartup).Assembly, typeof(IBuiltInCommandsAssemblyMarker).Assembly]);
    }

    [Benchmark]
    public async Task MoveMouseUp()
    {
        _dispatcher.Dispatch(new AudioTranscribedEvent("move mouse up 72 move mouse down 72 move mouse left 72 move mouse right 72"));
        await _dispatcher.FlushAsync();
    }

    public static async Task Test()
    {
        var container = new CommandBenchmarks();
        container.Setup();
        await container.MoveMouseUp();
    }
}