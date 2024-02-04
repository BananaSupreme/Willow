using Microsoft.Extensions.DependencyInjection;

using Willow.Eventing.Abstractions;
using Willow.Registration;

namespace Willow.Eventing.Registration;

public sealed class EventingRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IEventDispatcher, EventDispatcher>();
        services.AddSingleton<IUnsafeEventRegistrar>(static provider =>
                                                         (IUnsafeEventRegistrar)provider
                                                             .GetRequiredService<IEventDispatcher>());
    }
}
