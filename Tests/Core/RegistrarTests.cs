﻿using DryIoc.Microsoft.DependencyInjection;

using System.Reflection;
using System.Reflection.Emit;

using Willow.Core.Environment.Models;
using Willow.Core.Eventing.Abstractions;
using Willow.Core.Eventing.Registration;
using Willow.Core.Registration;
using Willow.Core.Registration.Abstractions;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.Models;

namespace Tests.Core;

public class RegistrarTests
{
    private readonly IInterfaceRegistrar _registrar;
    private readonly IEventRegistrar _eventRegistrar;
    private readonly IServiceProvider _serviceProvider;

    public RegistrarTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IInterfaceRegistrar, InterfaceRegistrar>();
        EventingServiceRegistrar.RegisterServices(services);
        services.AddSingleton<TestHelper>();
        _serviceProvider = services.CreateServiceProvider();
        _registrar = _serviceProvider.GetRequiredService<IInterfaceRegistrar>();
        _eventRegistrar = _serviceProvider.GetRequiredService<IEventRegistrar>();
    }

    [Fact]
    public async Task When_RegisteringNewEventFromAssembly_EventCorrectlyLoaded()
    {
        _eventRegistrar.RegisterFromAssemblies([typeof(TestEventHandler).Assembly]);
        var dispatcher = _serviceProvider.GetRequiredService<IEventDispatcher>();

        dispatcher.Dispatch(new Event());
        await dispatcher.FlushAsync();

        var helper = _serviceProvider.GetRequiredService<TestHelper>();
        helper.Ran.Should().BeTrue();
    }

    [Fact]
    public void When_RegisteringNewEventFromAssembly_ItsInterceptorsAreAlsoLoadedToIoC()
    {
        _eventRegistrar.RegisterFromAssemblies([typeof(TestEventHandler).Assembly]);
        _serviceProvider.Invoking(x => x.GetRequiredService<TestInterceptor>())
                        .Should().NotThrow();
    }


    [Fact]
    public void When_RegisteringNewInterface_ItIsLoadable()
    {
        _registrar.RegisterDeriving<INodeProcessor>([typeof(TestNodeProcessor).Assembly]);
        var processors = _serviceProvider.GetServices<INodeProcessor>();
        processors.Should().Contain(x => x.GetType() == typeof(TestNodeProcessor));
    }

    [Fact]
    public void When_NoTypesToBeRegistered_NoFailures()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("test"), AssemblyBuilderAccess.Run);
        _eventRegistrar.Invoking(x => x.RegisterFromAssemblies([assembly])).Should().NotThrow();
        _registrar.Invoking(x => x.RegisterDeriving<IInterfaceRegistrar>([])).Should().NotThrow();
    }

    [Fact]
    public void When_NoAssembliesToBeRegistered_NoFailures()
    {
        _eventRegistrar.Invoking(x => x.RegisterFromAssemblies([])).Should().NotThrow();
        _registrar.Invoking(x => x.RegisterDeriving<IInterfaceRegistrar>([])).Should().NotThrow();
    }
}

public record Event;

public class TestHelper
{
    public bool Ran { get; set; }
}

public class TestEventHandler : IEventHandler<Event>
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

public class TestNodeProcessor : INodeProcessor
{
    public bool IsLeaf => true;
    public uint Weight => 0;

    public NodeProcessingResult ProcessToken(ReadOnlyMemory<Token> tokens, CommandBuilder builder,
                                             Tag[] environmentTags)
    {
        return new(false, builder, tokens);
    }
}

public class TestInterceptor : IEventInterceptor<Event>
{
    public async Task InterceptAsync(Event @event, Func<Event, Task> next)
    {
        await next(@event);
    }
}