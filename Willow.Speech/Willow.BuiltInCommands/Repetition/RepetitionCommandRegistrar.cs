using Microsoft.Extensions.DependencyInjection;

using Willow.Registration;

namespace Willow.BuiltInCommands.Repetition;

internal sealed class RepetitionCommandRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<LastCommandContainer>();
    }
}
