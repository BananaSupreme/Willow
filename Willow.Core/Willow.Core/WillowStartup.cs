using DryIoc.Microsoft.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using Willow.Registration;

namespace Willow;

public static class WillowStartup
{
    public static async Task<IServiceProvider> StartAsync(IServiceCollection? services = null)
    {
        return await StartAsync(services, null);
    }

    internal static async Task<IServiceProvider> StartAsync(IServiceCollection? services,
                                                            Func<IServiceCollection, IServiceCollection>?
                                                                overrideServices)
    {
        services ??= new ServiceCollection();

        services.AddLogging();
        services.AddHttpClient();
        services.AddRegistration();

        overrideServices?.Invoke(services);

        var provider = services.CreateServiceProvider();

        var registrationEntry = provider.GetRequiredService<IAssemblyRegistrationEntry>();

        _ = await registrationEntry.RegisterAssemblyAsync(typeof(ICoreAssemblyMarker).Assembly);
        return provider;
    }
}
