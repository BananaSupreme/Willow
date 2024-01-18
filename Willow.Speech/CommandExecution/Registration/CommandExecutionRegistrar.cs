using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.CommandExecution.Abstraction;

namespace Willow.Speech.CommandExecution.Registration;

internal sealed class CommandExecutionRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ICommandStorage, CommandStorage>();
    }
}
