using Microsoft.Extensions.DependencyInjection;

using Willow.Registration;
using Willow.Speech.CommandExecution.Abstraction;

namespace Willow.Speech.CommandExecution.Registration;

internal sealed class CommandExecutionRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ICommandStorage, CommandStorage>();
    }
}
