using Tests.Helpers;

using Willow.Core.Eventing;
using Willow.Core.Eventing.Abstractions;

using Xunit.Abstractions;

// ReSharper disable MemberCanBePrivate.Global

namespace Tests.Core;

public sealed class EventDispatcherTests : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IEventDispatcher _sut;

    public EventDispatcherTests(ITestOutputHelper testOutputHelper)
    {
        var services = new ServiceCollection();
        services.AddTestLogger(testOutputHelper);
        services.AddSingleton<IEventDispatcher, EventDispatcher>();
        services.AddSingleton(typeof(TestEventHandler<>));
        services.AddSingleton(typeof(TestEventHandler2<>));
        services.AddSingleton(typeof(TestEventHandlerCalledCounter<>));
        services.AddSingleton(typeof(TestEventHandlerCalledCounter2<>));

        _provider = services.BuildServiceProvider();
        _sut = _provider.GetRequiredService<IEventDispatcher>();
    }

    public void Dispose()
    {
        _provider.Dispose();
    }

    [Fact]
    public async Task When_EventNotFlushed_StillCompletes()
    {
        var state = new StateWithTwoParameters();
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
        _sut.Dispatch(new StateWithTwoParameters());
        _sut.Flush();

        var handler = _provider.GetRequiredService<TestEventHandler<StateWithTwoParameters>>();
        var handler2 = _provider.GetRequiredService<TestEventHandler2<StateWithTwoParameters>>();

        handler.Called.Should().BeTrue();
        handler2.Called.Should().BeTrue();
    }

    [Fact]
    public void When_EventHandlerRegisteredMultipleTimes_RanOnce()
    {
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandlerCalledCounter<StateWithTwoParameters>>();
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandlerCalledCounter<StateWithTwoParameters>>();
        _sut.Dispatch(new StateWithTwoParameters());
        _sut.Flush();

        var handler = _provider.GetRequiredService<TestEventHandlerCalledCounter<StateWithTwoParameters>>();

        handler.Called.Should().Be(1);
    }

    [Fact]
    public void When_NonExistentHandlerRemoved_FailGracefully()
    {
        _sut.Invoking(static x => x.UnregisterHandler<StateWithTwoParameters, TestEventHandler<StateWithTwoParameters>>())
            .Should()
            .NotThrow();
    }

    [Fact]
    public void When_EventUnregistered_NotCalled()
    {
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandlerCalledCounter<StateWithTwoParameters>>();
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandlerCalledCounter2<StateWithTwoParameters>>();
        _sut.Dispatch(new StateWithTwoParameters());
        _sut.Flush();

        _sut.UnregisterHandler<StateWithTwoParameters, TestEventHandlerCalledCounter2<StateWithTwoParameters>>();
        _sut.Dispatch(new StateWithTwoParameters());
        _sut.Flush();

        var handler = _provider.GetRequiredService<TestEventHandlerCalledCounter<StateWithTwoParameters>>();
        var handler2 = _provider.GetRequiredService<TestEventHandlerCalledCounter2<StateWithTwoParameters>>();

        handler.Called.Should().Be(2);
        handler2.Called.Should().Be(1);
    }

    [Fact]
    public void When_MultipleEventsRegisteredAndOneFails_RestRunToCompletion()
    {
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler<StateWithTwoParameters>>();
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler2<StateWithTwoParameters>>();

        var handler = _provider.GetRequiredService<TestEventHandler<StateWithTwoParameters>>();
        var handler2 = _provider.GetRequiredService<TestEventHandler2<StateWithTwoParameters>>();

        handler.PerformAction = static _ => throw new Exception();

        _sut.Dispatch(new StateWithTwoParameters());
        _sut.Flush();

        handler.Called.Should().BeTrue();
        handler2.Called.Should().BeTrue();
    }

    public sealed record StateWithTwoParameters;

    internal sealed class TestEventHandler<T> : IEventHandler<T>
    {
        public Func<T, T>? PerformAction { get; set; }
        public bool Called { get; private set; }
        public T Event { get; private set; } = default!;

        public Task HandleAsync(T @event)
        {
            Event = @event;
            Called = true;
            PerformAction?.Invoke(@event);
            return Task.CompletedTask;
        }
    }

    internal sealed class TestEventHandler2<T> : IEventHandler<T>
    {
        public bool Called { get; private set; }

        public Task HandleAsync(T @event)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }

    internal sealed class TestEventHandlerCalledCounter<T> : IEventHandler<T>
    {
        public int Called { get; private set; }

        public Task HandleAsync(T @event)
        {
            Called++;
            return Task.CompletedTask;
        }
    }

    internal sealed class TestEventHandlerCalledCounter2<T> : IEventHandler<T>
    {
        public int Called { get; private set; }

        public Task HandleAsync(T @event)
        {
            Called++;
            return Task.CompletedTask;
        }
    }
}
