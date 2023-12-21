namespace Willow.Core.Eventing.Abstractions;

internal interface IUnsafeEventRegistrar
{
    void RegisterInterceptor(Type eventType, Type interceptor);
    void RegisterGenericInterceptor(Type interceptor);
    void RegisterHandler(Type eventType, Type eventHandler);
}