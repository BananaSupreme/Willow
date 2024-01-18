using Willow.Core.Eventing.Abstractions;

namespace Tests.Core.Eventing;

public partial class EventDispatcherTests
{
    public sealed record StateWithTwoParameters(int State1, int State2);

    public sealed record StateWithStringParameters(string State);

    internal sealed class TestGenericInterceptor : IGenericEventInterceptor
    {
        public bool Called { get; set; }

        public async Task InterceptAsync<TEvent>(TEvent @event, Func<TEvent, Task> next)
        {
            Called = true;
            await next(@event);
        }
    }

    internal sealed class TestInterceptor<T> : IEventInterceptor<T>
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

    internal sealed class TestInterceptor2<T> : IEventInterceptor<T>
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

    internal sealed class BlockingInterceptor<T> : IEventInterceptor<T>
    {
        public bool Called { get; private set; }

        public Task InterceptAsync(T @event, Func<T, Task> next)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }

    internal sealed class ExceptionCatchingInterceptor<T> : IEventInterceptor<T>
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
}
