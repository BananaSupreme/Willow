using Microsoft.Extensions.DependencyInjection;

using Willow.Core.CommandExecution.Abstraction;
using Willow.Core.Registration.Abstractions;

namespace Willow.Core.CommandExecution.Registration;

internal sealed class CommandExecutionRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ICommandStorage, CommandStorage>();
    }
}