using BenchmarkDotNet.Attributes;

using DryIoc.Microsoft.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Willow.BuiltInCommands;
using Willow.Core;
using Willow.Core.Eventing.Abstractions;
using Willow.Core.Registration.Abstractions;
using Willow.DeviceAutomation;
using Willow.DeviceAutomation.InputDevices.Abstractions;
using Willow.Speech;
using Willow.Speech.SpeechToText.Eventing.Events;
using Willow.StartUp;

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
        WillowStartup.Register(
            [typeof(ICoreAssemblyMarker).Assembly, typeof(ISpeechAssemblyMarker).Assembly],
            services);
        services.AddSingleton(_ => Substitute.For<IInputSimulator>());
        DeviceAutomationRegistrator.RegisterServices(services);
        var provider = services.CreateServiceProvider();
        _dispatcher = provider.GetRequiredService<IEventDispatcher>();
        var registrar = provider.GetRequiredService<IAssemblyRegistrationEntry>();
        registrar.RegisterAssemblies([
                                         typeof(ICoreAssemblyMarker).Assembly,
                                         typeof(ISpeechAssemblyMarker).Assembly,
                                         typeof(IBuiltInCommandsAssemblyMarker).Assembly
                                     ]);
    }

    [Benchmark]
    public void MoveMouseUp()
    {
        _dispatcher.Dispatch(
            new AudioTranscribedEvent("move mouse up 72 move mouse down 72 move mouse left 72 move mouse right 72"));
        _dispatcher.Flush();
    }

    public static void Test()
    {
        var container = new CommandBenchmarks();
        container.Setup();
        container.MoveMouseUp();
    }
}