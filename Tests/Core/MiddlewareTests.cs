using Tests.Helpers;

using Willow.Core.Middleware.Abstractions;
using Willow.Core.Middleware.Registration;

using Xunit.Abstractions;

namespace Tests.Core;

public sealed class MiddlewareTests : IDisposable
{
    private readonly IServiceProvider _provider;

    public MiddlewareTests(ITestOutputHelper outputHelper)
    {
        var services = new ServiceCollection();
        services.AddTestLogger(outputHelper);
        MiddlewareRegistrar.RegisterServices(services);
        services.AddSingleton(typeof(IMiddleware<>), typeof(TestMiddleware<>));
        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task When_NoMiddleware_StateRemainsTheSame()
    {
        var sut = _provider.GetRequiredService<IMiddlewareBuilderFactory<StateWithTwoParameters>>();
        var state = new StateWithTwoParameters(0, 0);
        await sut.Create().Build().ExecuteAsync(state, input => Task.FromResult(input.Should().Be(state)));
    }

    [Fact]
    public async Task When_SingleMiddlewareAsFunction_StateModifiedToWhatTheMiddlewareDictates()
    {
        var state = new StateWithTwoParameters(0, 0);
        var sut = _provider.GetRequiredService<IMiddlewareBuilderFactory<StateWithTwoParameters>>();
        var builder = sut.Create();
        builder.Add(static async (input, next) =>
        {
            var newInput = input with { Value2 = 10 };
            await next(newInput);
        });

        await builder.Build()
                     .ExecuteAsync(state,
                                   static input =>
                                   {
                                       input.Value1.Should().Be(0);
                                       input.Value2.Should().Be(10);
                                       return Task.CompletedTask;
                                   });
    }

    [Fact]
    public async Task When_SingleMiddlewareAsObject_StateModifiedToWhatTheMiddlewareDictates()
    {
        var state = new StateWithTwoParameters(0, 0);
        var sut = _provider.GetRequiredService<IMiddlewareBuilderFactory<StateWithTwoParameters>>();
        var middleware
            = (TestMiddleware<StateWithTwoParameters>)_provider
                .GetRequiredService<IMiddleware<StateWithTwoParameters>>();
        middleware.PerformAction = static (input) => input with { Value2 = 10 };
        var builder = sut.Create();
        await builder.Add(middleware)
                     .Build()
                     .ExecuteAsync(state,
                                   static input =>
                                   {
                                       input.Value1.Should().Be(0);
                                       input.Value2.Should().Be(10);
                                       return Task.CompletedTask;
                                   });
    }

    [Fact]
    public async Task When_SingleMiddlewareAsDefinition_StateModifiedToWhatTheMiddlewareDictates()
    {
        var state = new StateWithTwoParameters(0, 0);
        var sut = _provider.GetRequiredService<IMiddlewareBuilderFactory<StateWithTwoParameters>>();
        var middleware
            = (TestMiddleware<StateWithTwoParameters>)_provider
                .GetRequiredService<IMiddleware<StateWithTwoParameters>>();
        middleware.PerformAction = static (input) => input with { Value2 = 10 };
        var builder = sut.Create();
        await builder.Add<IMiddleware<StateWithTwoParameters>>()
                     .Build()
                     .ExecuteAsync(state,
                                   static input =>
                                   {
                                       input.Value1.Should().Be(0);
                                       input.Value2.Should().Be(10);
                                       return Task.CompletedTask;
                                   });
    }

    [Fact]
    public async Task When_MultipleMiddleware_AllModifyStateOneAfterTheOther()
    {
        var state = new StateWithStringParameter("");
        var sut = _provider.GetRequiredService<IMiddlewareBuilderFactory<StateWithStringParameter>>();
        await sut.Create()
                 .Add(static (input, next) => Task.FromResult(next(new StateWithStringParameter(input.Value + "1"))))
                 .Add(static (input, next) => Task.FromResult(next(new StateWithStringParameter(input.Value + "2"))))
                 .Add(static (input, next) => Task.FromResult(next(new StateWithStringParameter(input.Value + "3"))))
                 .Add(static (input, next) => Task.FromResult(next(new StateWithStringParameter(input.Value + "4"))))
                 .Add(static (input, next) => Task.FromResult(next(new StateWithStringParameter(input.Value + "5"))))
                 .Build()
                 .ExecuteAsync(state, static input => Task.FromResult(input.Should().BeEquivalentTo("12345")));
    }

    //This actually works with the previous test to show it happens since we're flipping the order of the interceptors
    [Fact]
    public async Task When_MultipleInterceptors_RunAtOrderRegistered()
    {
        var state = new StateWithStringParameter("");
        var sut = _provider.GetRequiredService<IMiddlewareBuilderFactory<StateWithStringParameter>>();
        await sut.Create()
                 .Add(static (input, next) => Task.FromResult(next(new StateWithStringParameter(input.Value + "5"))))
                 .Add(static (input, next) => Task.FromResult(next(new StateWithStringParameter(input.Value + "4"))))
                 .Add(static (input, next) => Task.FromResult(next(new StateWithStringParameter(input.Value + "3"))))
                 .Add(static (input, next) => Task.FromResult(next(new StateWithStringParameter(input.Value + "2"))))
                 .Add(static (input, next) => Task.FromResult(next(new StateWithStringParameter(input.Value + "1"))))
                 .Build()
                 .ExecuteAsync(state, static input => Task.FromResult(input.Should().BeEquivalentTo("54321")));
    }

    [Fact]
    public async Task When_MiddlewareDoesntCallNext_Abort()
    {
        var called = false;
        var state = new StateWithStringParameter("");
        var sut = _provider.GetRequiredService<IMiddlewareBuilderFactory<StateWithStringParameter>>();
        await sut.Create()
                 .Add(static (_, _) => Task.CompletedTask)
                 .Build()
                 .ExecuteAsync(state,
                               _ =>
                               {
                                   called = true;
                                   return Task.CompletedTask;
                               });
        called.Should().BeFalse();
    }

    [Fact]
    public async Task When_HandlerFails_MiddlewareCanRespond()
    {
        var called = false;
        var state = new StateWithStringParameter("");
        var sut = _provider.GetRequiredService<IMiddlewareBuilderFactory<StateWithStringParameter>>();
        var builder = sut.Create();
        builder.Add(async (input, next) =>
        {
            try
            {
                await next(input);
            }
            catch (Exception)
            {
                called = true;
            }
        });
        await builder.Build().ExecuteAsync(state, static _ => throw new Exception());
        called.Should().BeTrue();
    }

    public void Dispose()
    {
        if (_provider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private sealed record StateWithTwoParameters(int Value1, int Value2);

    private sealed record StateWithStringParameter(string Value);

    internal sealed class TestMiddleware<T> : IMiddleware<T>
    {
        public Func<T, T>? PerformAction { get; set; }
        public bool Called { get; private set; }

        public async Task ExecuteAsync(T input, Func<T, Task> next)
        {
            if (PerformAction != null)
            {
                input = PerformAction.Invoke(input);
            }

            Called = true;
            await next(input);
        }
    }
}
