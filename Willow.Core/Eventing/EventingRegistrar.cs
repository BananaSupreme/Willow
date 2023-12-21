using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.Helpers;
using Willow.Core.SpeechCommands.ScriptingInterface;
using Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;

namespace Willow.Core.Eventing;

public class EventingRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IEventRegistrar, EventRegistrar>();
        services.AddSingleton<IEventDispatcher, EventDispatcher>();
        services.AddSingleton<IUnsafeEventRegistrar>(provider => (IUnsafeEventRegistrar)provider.GetRequiredService<IEventDispatcher>());
    }
}