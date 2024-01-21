using System.Reflection;
using System.Reflection.Emit;

using DryIoc.Microsoft.DependencyInjection;

using Tests.Helpers;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.Eventing.Registration;
using Willow.Core.Registration;
using Willow.Core.Registration.Abstractions;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

using Xunit.Abstractions;

namespace Tests.Core;

public sealed class RegistrarTests : IDisposable
{
    private readonly IEventRegistrar _eventRegistrar;
    private readonly IInterfaceRegistrar _registrar;
    private readonly IServiceProvider _serviceProvider;

    public RegistrarTests(ITestOutputHelper testOutputHelper)
    {
        var services = new ServiceCollection();
        services.AddTestLogger(testOutputHelper);
        services.AddSingleton<IInterfaceRegistrar, InterfaceRegistrar>();
        EventingRegistrar.RegisterServices(services);
        services.AddSingleton<TestHelper>();
        _serviceProvider = services.CreateServiceProvider();
        _registrar = _serviceProvider.GetRequiredService<IInterfaceRegistrar>();
        _eventRegistrar = _serviceProvider.GetRequiredService<IEventRegistrar>();
    }

    public void Dispose()
    {
        (_serviceProvider as IDisposable)?.Dispose();
    }

    [Fact]
    public void When_RegisteringNewEventFromAssembly_EventCorrectlyLoaded()
    {
        _eventRegistrar.RegisterFromAssemblies([typeof(TestEventHandler).Assembly]);
        var dispatcher = _serviceProvider.GetRequiredService<IEventDispatcher>();

        dispatcher.Dispatch(new Event(Guid.NewGuid()));
        dispatcher.Flush();

        var helper = _serviceProvider.GetRequiredService<TestHelper>();
        helper.Ran.Should().BeTrue();
    }

    [Fact]
    public void When_RegisteringNewInterface_ItIsLoadable()
    {
        _registrar.RegisterDeriving<INodeProcessor>([typeof(TestNodeProcessor).Assembly]);
        var processors = _serviceProvider.GetServices<INodeProcessor>();
        processors.Should().Contain(static x => x is TestNodeProcessor);
    }

    [Fact]
    public void When_NoTypesToBeRegistered_NoFailures()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("test"), AssemblyBuilderAccess.Run);
        _eventRegistrar.Invoking(x => x.RegisterFromAssemblies([assembly])).Should().NotThrow();
        _registrar.Invoking(static x => x.RegisterDeriving<IInterfaceRegistrar>([])).Should().NotThrow();
    }

    [Fact]
    public void When_NoAssembliesToBeRegistered_NoFailures()
    {
        _eventRegistrar.Invoking(static x => x.RegisterFromAssemblies([])).Should().NotThrow();
        _registrar.Invoking(static x => x.RegisterDeriving<IInterfaceRegistrar>([])).Should().NotThrow();
    }
}

// ReSharper disable once NotAccessedPositionalProperty.Global
public sealed record Event(Guid Id);

public sealed class TestHelper
{
    public bool Ran { get; set; }
}

public sealed class TestEventHandler : IEventHandler<Event>
{
    private readonly TestHelper _helper;

    public TestEventHandler(TestHelper helper)
    {
        _helper = helper;
    }

    public Task HandleAsync(Event @event)
    {
        _helper.Ran = true;
        return Task.CompletedTask;
    }
}

public sealed class TestNodeProcessor : INodeProcessor
{
    public bool IsLeaf => true;
    public uint Weight => 0;

    public TokenProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder)
    {
        return new TokenProcessingResult(false, builder, tokens);
    }
}
