using Willow.Core.Eventing;
using Willow.Core.Eventing.Abstractions;

// ReSharper disable MemberCanBePrivate.Global

namespace Tests.Core.Eventing;

public sealed partial class EventDispatcherTests : IDisposable
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
    public void When_MultipleEventHandlersRegistered_AllRun()
    {
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler<StateWithTwoParameters>>();
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler2<StateWithTwoParameters>>();
        _sut.Dispatch(new StateWithTwoParameters(0, 0));
        _sut.Flush();

        var handler = _provider.GetRequiredService<TestEventHandler<StateWithTwoParameters>>();
        var handler2 = _provider.GetRequiredService<TestEventHandler2<StateWithTwoParameters>>();

        handler.Called.Should().BeTrue();
        handler2.Called.Should().BeTrue();
    }

    [Fact]
    public void When_MultipleEventsRegisteredAndOneFails_RestRunToCompletion()
    {
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler<StateWithTwoParameters>>();
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler2<StateWithTwoParameters>>();

        var handler = _provider.GetRequiredService<TestEventHandler<StateWithTwoParameters>>();
        var handler2 = _provider.GetRequiredService<TestEventHandler2<StateWithTwoParameters>>();

        handler.PerformAction = _ => throw new Exception();

        _sut.Dispatch(new StateWithTwoParameters(0, 0));
        _sut.Flush();

        handler.Called.Should().BeTrue();
        handler2.Called.Should().BeTrue();
    }

    [Fact]
    public void When_EventHandlerFails_InterceptorCanRespond()
    {
        _sut.RegisterHandler<StateWithStringParameters, TestEventHandler<StateWithStringParameters>>();
        _sut.RegisterInterceptor<StateWithStringParameters, ExceptionCatchingInterceptor<StateWithStringParameters>>();

        var handler = _provider.GetRequiredService<TestEventHandler<StateWithStringParameters>>();
        var interceptor = _provider.GetRequiredService<ExceptionCatchingInterceptor<StateWithStringParameters>>();
        handler.PerformAction = _ => throw new Exception();

        _sut.Dispatch(new StateWithStringParameters(""));
        _sut.Flush();

        handler.Called.Should().BeTrue();
        interceptor.Called.Should().BeTrue();
    }

    public void Dispose()
    {
        _provider.Dispose();
    }
}