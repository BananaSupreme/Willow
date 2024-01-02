using Willow.Core.Eventing;
using Willow.Core.Eventing.Abstractions;

// ReSharper disable MemberCanBePrivate.Global

namespace Tests.Core.Eventing;

public partial class EventDispatcherTests
{
    private readonly IEventDispatcher _sut;
    private readonly ServiceProvider _provider;

    public EventDispatcherTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IEventDispatcher, EventDispatcher>();
        services.AddSingleton(typeof(TestInterceptor<>));
        services.AddSingleton(typeof(TestInterceptor2<>));
        services.AddSingleton(typeof(ExceptionCatchingInterceptor<>));
        services.AddSingleton(typeof(BlockingInterceptor<>));
        services.AddSingleton(typeof(TestEventHandler<>));
        services.AddSingleton(typeof(TestEventHandler2<>));
        services.AddSingleton<TestGenericInterceptor>();

        _provider = services.BuildServiceProvider();
        _sut = _provider.GetRequiredService<IEventDispatcher>();
    }

    [Fact]
    public async Task When_EventNotFlushed_StillCompletes()
    {
        var state = new StateWithTwoParameters(0, 0);
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler<StateWithTwoParameters>>();
        _sut.Dispatch(state);

        await Task.Delay(1000);
        var handler = _provider.GetRequiredService<TestEventHandler<StateWithTwoParameters>>();
        state.Should().BeEquivalentTo(handler.Event);
    }
    
    [Fact]
    public async Task When_MultipleEventHandlersRegistered_AllRun()
    {
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler<StateWithTwoParameters>>();
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler2<StateWithTwoParameters>>();
        _sut.Dispatch(new StateWithTwoParameters(0, 0));
        await _sut.FlushAsync();

        var handler = _provider.GetRequiredService<TestEventHandler<StateWithTwoParameters>>();
        var handler2 = _provider.GetRequiredService<TestEventHandler2<StateWithTwoParameters>>();

        handler.Called.Should().BeTrue();
        handler2.Called.Should().BeTrue();
    }

    [Fact]
    public async Task When_MultipleEventsRegisteredAndOneFails_RestRunToCompletion()
    {
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler<StateWithTwoParameters>>();
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler2<StateWithTwoParameters>>();

        var handler = _provider.GetRequiredService<TestEventHandler<StateWithTwoParameters>>();
        var handler2 = _provider.GetRequiredService<TestEventHandler2<StateWithTwoParameters>>();

        handler.PerformAction = _ => throw new Exception();

        _sut.Dispatch(new StateWithTwoParameters(0, 0));
        await _sut.FlushAsync();

        handler.Called.Should().BeTrue();
        handler2.Called.Should().BeTrue();
    }

    [Fact]
    public async Task When_EventHandlerFails_InterceptorCanRespond()
    {
        _sut.RegisterHandler<StateWithStringParameters, TestEventHandler<StateWithStringParameters>>();
        _sut.RegisterInterceptor<StateWithStringParameters, ExceptionCatchingInterceptor<StateWithStringParameters>>();

        var handler = _provider.GetRequiredService<TestEventHandler<StateWithStringParameters>>();
        var interceptor = _provider.GetRequiredService<ExceptionCatchingInterceptor<StateWithStringParameters>>();
        handler.PerformAction = _ => throw new Exception();

        _sut.Dispatch(new StateWithStringParameters(""));
        await _sut.FlushAsync();

        handler.Called.Should().BeTrue();
        interceptor.Called.Should().BeTrue();
    }
}