using Willow.Core.Eventing;
using Willow.Core.Eventing.Abstractions;

// ReSharper disable MemberCanBePrivate.Global

namespace Tests.Core;

public class EventDispatcherTests
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
    public async Task When_NoEventConsumerRegisteredAndInterceptorsRegistered_InterceptorNotRun()
    {
        _sut.RegisterInterceptor<StateWithTwoParameters, TestInterceptor<StateWithTwoParameters>>();
        _sut.Dispatch(new StateWithTwoParameters(1, 0));
        await _sut.FlushAsync();

        _provider.GetRequiredService<TestInterceptor<StateWithTwoParameters>>()
                 .Called.Should().BeFalse();
    }

    [Fact]
    public async Task When_NoInterceptors_StateRemainsTheSame()
    {
        var state = new StateWithTwoParameters(0, 0);
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler<StateWithTwoParameters>>();
        _sut.Dispatch(state);
        await _sut.FlushAsync();

        var handler = _provider.GetRequiredService<TestEventHandler<StateWithTwoParameters>>();
        state.Should().BeEquivalentTo(handler.Event);
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
    public async Task When_SingleInterceptor_StateModifiedToWhatTheInterceptorDictated()
    {
        var state = new StateWithTwoParameters(0, 0);
        _sut.RegisterInterceptor<StateWithTwoParameters, TestInterceptor<StateWithTwoParameters>>();
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler<StateWithTwoParameters>>();

        var interceptor = _provider.GetRequiredService<TestInterceptor<StateWithTwoParameters>>();
        interceptor.PerformAction = x => x with { State2 = 10 };

        _sut.Dispatch(state);
        await _sut.FlushAsync();
        
        var handler = _provider.GetRequiredService<TestEventHandler<StateWithTwoParameters>>();
        handler.Event.State1.Should().Be(state.State1);
        handler.Event.State2.Should().Be(10);
    }

    [Fact]
    public async Task When_MultipleInterceptors_AllModifyStateOneAfterTheOther()
    {
        var state = new StateWithStringParameters("");
        _sut.RegisterInterceptor<StateWithStringParameters, TestInterceptor<StateWithStringParameters>>();
        _sut.RegisterInterceptor<StateWithStringParameters, TestInterceptor2<StateWithStringParameters>>();
        _sut.RegisterHandler<StateWithStringParameters, TestEventHandler<StateWithStringParameters>>();

        var interceptor = _provider.GetRequiredService<TestInterceptor<StateWithStringParameters>>();
        interceptor.PerformAction = x => new(x.State + "1");

        var interceptor2 = _provider.GetRequiredService<TestInterceptor2<StateWithStringParameters>>();
        interceptor2.PerformAction = x => new(x.State + "2");

        _sut.Dispatch(state);
        await _sut.FlushAsync();

        var handler = _provider.GetRequiredService<TestEventHandler<StateWithStringParameters>>();
        interceptor.Called.Should().BeTrue();
        interceptor2.Called.Should().BeTrue();
        handler.Event.State.Should().Be("12");
    }

    //This actually works with the previous test to show it happens since we're flipping the order of the interceptors
    [Fact]
    public async Task When_MultipleInterceptors_RunAtOrderRegistered()
    {
        var state = new StateWithStringParameters("");
        _sut.RegisterInterceptor<StateWithStringParameters,
            TestInterceptor2<StateWithStringParameters>>(); // ! Note order flipped
        _sut.RegisterInterceptor<StateWithStringParameters, TestInterceptor<StateWithStringParameters>>();
        _sut.RegisterHandler<StateWithStringParameters, TestEventHandler<StateWithStringParameters>>();

        var interceptor = _provider.GetRequiredService<TestInterceptor<StateWithStringParameters>>();
        interceptor.PerformAction = x => new(x.State + "1");

        var interceptor2 = _provider.GetRequiredService<TestInterceptor2<StateWithStringParameters>>();
        interceptor2.PerformAction = x => new(x.State + "2");

        _sut.Dispatch(state);
        await _sut.FlushAsync();

        var handler = _provider.GetRequiredService<TestEventHandler<StateWithStringParameters>>();
        handler.Event.State.Should().Be("21");
    }

    //For an edge case that was found in development
    [Fact]
    public async Task When_OneInterceptor_ModifyOnce()
    {
        var state = new StateWithStringParameters("");
        _sut.RegisterInterceptor<StateWithStringParameters, TestInterceptor<StateWithStringParameters>>();
        _sut.RegisterHandler<StateWithStringParameters, TestEventHandler<StateWithStringParameters>>();

        var interceptor = _provider.GetRequiredService<TestInterceptor<StateWithStringParameters>>();
        interceptor.PerformAction = x => new(x.State + "1");

        _sut.Dispatch(state);
        await _sut.FlushAsync();

        var handler = _provider.GetRequiredService<TestEventHandler<StateWithStringParameters>>();
        handler.Event.State.Should().Be("1");
    }

    [Fact]
    public async Task When_RegisteringUnrelatedInterceptor_Ignored()
    {
        var state = new StateWithTwoParameters(0, 0);
        _sut.RegisterInterceptor<StateWithStringParameters, TestInterceptor<StateWithStringParameters>>();
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler<StateWithTwoParameters>>();
        _sut.Dispatch(state);
        await _sut.FlushAsync();

        var interceptor = _provider.GetRequiredService<TestInterceptor<StateWithStringParameters>>();
        var handler = _provider.GetRequiredService<TestEventHandler<StateWithTwoParameters>>();

        interceptor.Called.Should().BeFalse();
        handler.Called.Should().BeTrue();
    }

    [Fact]
    public async Task When_InterceptorDoesntCallNext_EventNotProcessed()
    {
        var state = new StateWithTwoParameters(0, 0);
        _sut.RegisterInterceptor<StateWithTwoParameters, BlockingInterceptor<StateWithTwoParameters>>();
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler<StateWithTwoParameters>>();
        _sut.Dispatch(state);
        await _sut.FlushAsync();

        var interceptor = _provider.GetRequiredService<BlockingInterceptor<StateWithTwoParameters>>();
        var handler = _provider.GetRequiredService<TestEventHandler<StateWithTwoParameters>>();

        interceptor.Called.Should().BeTrue();
        handler.Called.Should().BeFalse();
    }

    [Fact]
    public async Task When_RegisteringGenericInterceptor_AppliesToAllEvents()
    {
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler<StateWithTwoParameters>>();
        _sut.RegisterHandler<StateWithStringParameters, TestEventHandler<StateWithStringParameters>>();
        _sut.RegisterGenericInterceptor<TestGenericInterceptor>();

        var interceptor = _provider.GetRequiredService<TestGenericInterceptor>();

        _sut.Dispatch(new StateWithStringParameters(""));
        await _sut.FlushAsync();

        interceptor.Called.Should().BeTrue();
        interceptor.Called = false;

        _sut.Dispatch(new StateWithTwoParameters(0, 0));
        await _sut.FlushAsync();
        interceptor.Called.Should().BeTrue();
    }

    [Fact]
    public async Task When_RegisteringGenericInterceptor_GenericRunsFirst()
    {
        _sut.RegisterHandler<StateWithTwoParameters, TestEventHandler<StateWithTwoParameters>>();
        _sut.RegisterInterceptor<StateWithTwoParameters, TestInterceptor<StateWithTwoParameters>>();
        _sut.RegisterGenericInterceptor<TestGenericInterceptor>();

        var genericInterceptor = _provider.GetRequiredService<TestGenericInterceptor>();
        var interceptor = _provider.GetRequiredService<TestInterceptor<StateWithTwoParameters>>();

        interceptor.PerformAction = x =>
        {
            genericInterceptor.Called.Should().BeTrue();
            return x;
        };

        _sut.Dispatch(new StateWithTwoParameters(0, 0));
        await _sut.FlushAsync();
        interceptor.Called.Should().BeTrue();
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

    public record StateWithTwoParameters(int State1, int State2);

    public record StateWithStringParameters(string State);

    internal class TestGenericInterceptor : IGenericEventInterceptor
    {
        public bool Called { get; set; }

        public async Task InterceptAsync<TEvent>(TEvent @event, Func<TEvent, Task> next)
        {
            Called = true;
            await next(@event);
        }
    }

    internal class TestInterceptor<T> : IEventInterceptor<T>
    {
        public Func<T, T>? PerformAction { get; set; }
        public bool Called { get; private set; }

        public async Task InterceptAsync(T @event, Func<T, Task> next)
        {
            if (PerformAction != null)
            {
                @event = PerformAction.Invoke(@event);
            }
            Called = true;
            await next(@event);
        }
    }

    internal class TestInterceptor2<T> : IEventInterceptor<T>
    {
        public Func<T, T>? PerformAction { get; set; }
        public bool Called { get; private set; }

        public async Task InterceptAsync(T @event, Func<T, Task> next)
        {
            if (PerformAction != null)
            {
                @event = PerformAction.Invoke(@event);
            }

            Called = true;
            await next(@event);
        }
    }

    internal class BlockingInterceptor<T> : IEventInterceptor<T>
    {
        public bool Called { get; private set; }

        public Task InterceptAsync(T @event, Func<T, Task> next)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }

    internal class ExceptionCatchingInterceptor<T> : IEventInterceptor<T>
    {
        public bool Called { get; private set; }

        public async Task InterceptAsync(T @event, Func<T, Task> next)
        {
            try
            {
                await next(@event);
            }
            catch
            {
                Called = true;
            }
        }
    }

    internal class TestEventHandler<T> : IEventHandler<T>
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

    internal class TestEventHandler2<T> : IEventHandler<T>
    {
        public bool Called { get; private set; }

        public Task HandleAsync(T @event)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }
}