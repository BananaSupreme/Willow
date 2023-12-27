using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;

namespace Willow.Core.Registration;

internal class RegistrationRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAssemblyRegistrationEntry, AssemblyRegistrationEntry>();
        services.AddSingleton<IInterfaceRegistrar, InterfaceRegistrar>();
    }
}