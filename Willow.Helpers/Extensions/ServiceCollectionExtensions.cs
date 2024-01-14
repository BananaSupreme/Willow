using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Willow.Helpers.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Searches the types own assembly for all the concrete implementations and registers them with the specified lifetime.
    /// </summary>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <param name="services">The DI service collection.</param>
    /// <param name="lifetime">The registration lifetime requested.</param>
    /// <typeparam name="T">The type to search implementors of.</typeparam>
    /// <exception cref="InvalidOperationException"><typeparamref name="T"/> must be an interface type.</exception>
    public static IServiceCollection AddAllTypesFromOwnAssembly<T>(this IServiceCollection services,
                                                                   ServiceLifetime lifetime)
    {
        return services.AddAllTypesFromAssemblyMarked<T, T>(lifetime);
    }

    /// <summary>
    /// Searches the types own assembly for all the concrete implementations and registers them with the specified lifetime.
    /// </summary>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <param name="services">The DI service collection.</param>
    /// <param name="lifetime">The registration lifetime requested.</param>
    /// <typeparam name="T">The type to search implementors of.</typeparam>
    /// <typeparam name="TMarker">A marker for the assembly that should be searched.</typeparam>
    /// <exception cref="InvalidOperationException"><typeparamref name="T"/> must be an interface type.</exception>
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