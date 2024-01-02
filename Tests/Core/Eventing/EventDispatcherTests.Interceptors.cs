namespace Tests.Core.Eventing;

public partial class EventDispatcherTests
{
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
}