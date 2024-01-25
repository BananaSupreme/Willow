using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace Willow.Helpers.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Searches the assembly for all the concrete implementations of the type and registers them.
    /// </summary>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <param name="services">The DI service collection.</param>
    /// <param name="assembly">The assembly to look for types in.</param>
    /// <typeparam name="T">The type to search implementors of.</typeparam>
    /// <exception cref="InvalidOperationException"><typeparamref name="T" /> must be an interface type.</exception>
    public static IServiceCollection AddAllTypesDeriving<T>(this IServiceCollection services, Assembly assembly)
    {
        if (!typeof(T).IsInterface)
        {
            throw new InvalidOperationException("T should be an interface type");
        }

        var types = assembly.GetAllDeriving<T>();
        foreach (var type in types)
        {
            services.AddSingleton(typeof(T), type);
        }

        return services;
    }

    /// <summary>
    /// Searches the types own assembly for all the concrete implementations and registers them with the specified
    /// lifetime.
    /// </summary>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <param name="services">The DI service collection.</param>
    /// <typeparam name="T">The type to search implementors of.</typeparam>
    /// <exception cref="InvalidOperationException"><typeparamref name="T" /> must be an interface type.</exception>
    public static IServiceCollection AddAllTypesFromOwnAssembly<T>(this IServiceCollection services)
    {
        return services.AddAllTypesFromAssemblyMarked<T, T>();
    }

    /// <summary>
    /// Searches the types own assembly for all the concrete implementations and registers them with the specified
    /// lifetime.
    /// </summary>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <param name="services">The DI service collection.</param>
    /// <typeparam name="T">The type to search implementors of.</typeparam>
    /// <typeparam name="TMarker">A marker for the assembly that should be searched.</typeparam>
    /// <exception cref="InvalidOperationException"><typeparamref name="T" /> must be an interface type.</exception>
    public static IServiceCollection AddAllTypesFromAssemblyMarked<T, TMarker>(this IServiceCollection services)
    {
        return services.AddAllTypesDeriving<T>(typeof(TMarker).Assembly);
    }

    /// <summary>
    /// Searches the assembly for all the concrete implementations of the type and registers them as a mapping.
    /// </summary>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <param name="services">The DI service collection.</param>
    /// <param name="assembly">The assembly to look for types in.</param>
    /// <typeparam name="T">The type to search implementors of.</typeparam>
    /// <exception cref="InvalidOperationException"><typeparamref name="T" /> must be an interface type.</exception>
    public static IServiceCollection AddAllTypesDerivingAsMapping<T>(this IServiceCollection services, Assembly assembly)
    {
        if (!typeof(T).IsInterface)
        {
            throw new InvalidOperationException("T should be an interface type");
        }

        var types = assembly.GetAllDeriving<T>();
        foreach (var type in types)
        {
            services.AddSingleton(type);
            services.AddSingleton(typeof(T), provider => provider.GetRequiredService(type));
        }

        return services;
    }

    /// <summary>
    /// Searches the assembly for all the concrete implementations of the type and registers them as a mapping.
    /// </summary>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <param name="services">The DI service collection.</param>
    /// <typeparam name="T">The type to search implementors of.</typeparam>
    /// <exception cref="InvalidOperationException"><typeparamref name="T" /> must be an interface type.</exception>
    public static IServiceCollection AddAllTypesAsMappingFromOwnAssembly<T>(this IServiceCollection services)
    {
        return services.AddAllTypesAsMappingFromAssemblyMarked<T, T>();
    }

    /// <summary>
    /// Searches the assembly for all the concrete implementations of the type and registers them as a mapping.
    /// </summary>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <param name="services">The DI service collection.</param>
    /// <typeparam name="T">The type to search implementors of.</typeparam>
    /// <typeparam name="TMarker">A marker for the assembly that should be searched.</typeparam>
    /// <exception cref="InvalidOperationException"><typeparamref name="T" /> must be an interface type.</exception>
    public static IServiceCollection AddAllTypesAsMappingFromAssemblyMarked<T, TMarker>(this IServiceCollection services)
    {
        return services.AddAllTypesDerivingAsMapping<T>(typeof(TMarker).Assembly);
    }
}
