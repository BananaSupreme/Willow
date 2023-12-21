using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Willow.Core.Helpers.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAllTypesFromOwnAssembly<T>(this IServiceCollection services,
                                                                   ServiceLifetime lifetime)
    {
        return services.AddAllTypesFromAssemblyMarked<T, T>(lifetime);
    }

    public static IServiceCollection AddAllTypesFromAssemblyMarked<T, TMarker>(
        this IServiceCollection services, ServiceLifetime lifetime)

    {
        if (!typeof(T).IsInterface)
        {
            throw new InvalidOperationException("T should be an interface type");
        }

        var assembly = typeof(TMarker).Assembly;
        var types = assembly.GetTypes()
                            .Where(x => !x.IsNestedPrivate && !x.IsAbstract && !x.IsInterface
                                        && x.GetInterfaces().Contains(typeof(T)));

        var descriptors = types.Select(x => new ServiceDescriptor(typeof(T), x, lifetime));

        return services.Add(descriptors);
    }
}