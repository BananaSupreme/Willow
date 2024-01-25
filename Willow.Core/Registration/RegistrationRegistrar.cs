using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;

namespace Willow.Core.Registration;

internal static class RegistrationRegistrar
{
    public static void AddRegistration(this IServiceCollection services)
    {
        services.AddSingleton(typeof(ICollectionProvider<>), typeof(CollectionProvider<>));
        services.AddSingleton<IAssemblyRegistrationEntry, AssemblyRegistrationEntry>();
    }
}
