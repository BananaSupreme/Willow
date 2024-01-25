using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.Registration.Abstractions;

namespace Willow.Core.Eventing.Registration;

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
