namespace Willow.Core.Eventing.Abstractions;

internal interface IInterceptorRegistrar
{
    static abstract void RegisterInterceptor(IEventDispatcher eventDispatcher);
}