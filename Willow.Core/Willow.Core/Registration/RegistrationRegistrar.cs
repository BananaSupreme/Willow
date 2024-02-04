using Microsoft.Extensions.DependencyInjection;

namespace Willow.Registration;

internal static class RegistrationRegistrar
{
    public static void AddRegistration(this IServiceCollection services)
    {
        services.AddSingleton(typeof(ICollectionProvider<>), typeof(CollectionProvider<>));
        services.AddSingleton<IAssemblyRegistrationEntry, AssemblyRegistrationEntry>();
    }
}
