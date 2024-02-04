using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Willow.Helpers.Extensions;

namespace Willow.Registration;

internal sealed class ServiceAssemblyRegistrar : IAssemblyRegistrar
{
    public void Register(Assembly assembly, Guid assemblyId, IServiceCollection services)
    {
        var registrars = assembly.GetAllDeriving<IServiceRegistrar>();
        var activatedServiceRegistrar = registrars.Select(Activator.CreateInstance).Cast<IServiceRegistrar>();
        foreach (var serviceRegistrar in activatedServiceRegistrar)
        {
            serviceRegistrar.RegisterServices(services);
        }
    }

    public Task StartAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        return Task.CompletedTask;
    }
}
