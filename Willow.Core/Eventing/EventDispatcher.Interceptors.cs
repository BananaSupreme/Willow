using Willow.Core.Eventing.Abstractions;
using Willow.Helpers.Extensions;
using Willow.Helpers.Logging.Extensions;

namespace Willow.Core.Eventing;

internal sealed partial class EventDispatcher
{
    private const string GenericInterceptorName = "Base";
    private readonly Dictionary<string, List<Type>> _interceptorsStorage = [];

    public void RegisterInterceptor<TEvent, TInterceptor>()
        where TInterceptor : IEventInterceptor<TEvent> where TEvent : notnull
    {
        RegisterInterceptor(typeof(TEvent), typeof(TInterceptor));
    }

    public void RegisterGenericInterceptor<TGenericEventInterceptor>()
        where TGenericEventInterceptor : IGenericEventInterceptor
    {
        RegisterGenericInterceptor(typeof(TGenericEventInterceptor));
    }

    public void RegisterInterceptor(Type eventType, Type interceptor)
    {
        var eventName = TypeExtensions.GetFullName(eventType);
        RegisterInterceptor(eventName, interceptor);
    }

    public void RegisterGenericInterceptor(Type interceptor)
    {
        RegisterInterceptor(GenericInterceptorName, interceptor);
    }

    private void RegisterInterceptor(string eventName, Type interceptorType)
    {
        _log.InterceptorRegistering(TypeExtensions.GetFullName(interceptorType), eventName);
        if (_interceptorsStorage.TryGetValue(eventName, out var interceptors))
        {
            interceptors.Add(interceptorType);
            return;
        }

        _interceptorsStorage.Add(eventName, [interceptorType]);
    }

    private Func<TEvent, Task> GetNextInterceptor<TEvent>(List<InterceptorFunction<TEvent>> interceptors,
                                                          List<IEventHandler<TEvent>> handlers,
                                                          int currentIndex) where TEvent : notnull
    {
        if (currentIndex < interceptors.Count - 1)
        {
            var nextInterceptor = interceptors[currentIndex];
            return @event => RunInterceptorLogged(nextInterceptor,
                                                  @event,
                                                  GetNextInterceptor(interceptors, handlers, currentIndex + 1));
        }

        var lastInterceptor = interceptors[^1];
        return @event => RunInterceptorLogged(lastInterceptor, @event, newEvent => RunEvents(handlers, newEvent));
    }

    private async Task RunInterceptorLogged<TEvent>(InterceptorFunction<TEvent> interceptor,
                                                    TEvent @event,
                                                    Func<TEvent, Task> next)
    {
        using var _ = _log.AddContext("interceptorName", TypeExtensions.GetFullName(interceptor.GetType()));
        _log.InterceptorExecutionStarting();
        try
        {
            await interceptor(@event, next);
        }
        catch (Exception ex)
        {
            _log.InterceptorHandlingError(ex);
        }
    }

    private List<InterceptorFunction<TEvent>> GetInterceptors<TEvent>()
    {
        List<InterceptorFunction<TEvent>> result = [];
        if (_interceptorsStorage.TryGetValue(GenericInterceptorName, out var genericInterceptors))
        {
            AddGenericInterceptors(genericInterceptors, result);
        }

        if (_interceptorsStorage.TryGetValue(TypeExtensions.GetFullName<TEvent>(), out var interceptors))
        {
            AddConcreteInterceptors(interceptors, result);
        }

        return result;
    }

    private void AddConcreteInterceptors<TEvent>(List<Type> interceptors, List<InterceptorFunction<TEvent>> result)
    {
        var actualized = interceptors.Select(Actualize<IEventInterceptor<TEvent>>).ToList();
        var toFunction
            = actualized.Select<IEventInterceptor<TEvent>, InterceptorFunction<TEvent>>(
                static x => async (@event, next) => await x.InterceptAsync(@event, next));
        result.AddRange(toFunction);
    }

    private void AddGenericInterceptors<TEvent>(List<Type> genericInterceptors, List<InterceptorFunction<TEvent>> result)
    {
        var actualized = genericInterceptors.Select(Actualize<IGenericEventInterceptor>).ToList();
        var toFunction
            = actualized.Select<IGenericEventInterceptor, InterceptorFunction<TEvent>>(
                static x => async (@event, next) => await x.InterceptAsync(@event, next));
        result.AddRange(toFunction);
    }

    private delegate Task InterceptorFunction<TEvent>(TEvent @event, Func<TEvent, Task> next);
}
