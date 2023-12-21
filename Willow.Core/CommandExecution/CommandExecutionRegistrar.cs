using Microsoft.Extensions.DependencyInjection;

using Willow.Core.CommandExecution.Abstraction;
using Willow.Core.Helpers;

namespace Willow.Core.CommandExecution;

internal class CommandExecutionRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ICommandStorage, CommandStorage>();
    }
}