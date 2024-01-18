using System.Reflection;

namespace Willow.Helpers.Extensions;

public static class TypeExtensions
{
    /// <summary>
    /// Returns the fullname or name if full name does not exist of <typeparamref name="T" />
    /// </summary>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <typeparam name="T">the type to provide the name of</typeparam>
    /// <returns>The full name or name if full name does not exist</returns>
    public static string GetFullName<T>()
    {
        return GetFullName(typeof(T));
    }

    /// <summary>
    /// Returns the fullname or name if full name does not exist of <paramref name="t" />
    /// </summary>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <param name="t">the type to provide the name of</param>
    /// <returns>The full name or name if full name does not exist</returns>
    public static string GetFullName(Type t)
    {
        return t.FullName ?? t.Name;
    }

    /// <summary>
    /// Finds all concrete types deriving <paramref name="type" /> in the assembly requested.
    /// </summary>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <param name="type">The type to look for concrete derivations of.</param>
    /// <param name="assembly">The assembly to search the types in.</param>
    /// <returns>Concrete types assignable to <paramref name="type" />.</returns>
    public static Type[] GetAllDerivingInAssembly(this Type type, Assembly assembly)
    {
        return assembly.GetTypes().Where(static t => t.IsConcrete()).Where(t => t.IsAssignableTo(type)).ToArray();
    }

    /// <summary>
    /// Tests whether a type derives from an open generic type definitions, even when specifying a specific type.
    /// </summary>
    /// <example>
    /// <c>XEventHandler : EventHandler{T}</c> will return true if tested as
    /// <c>XEventHandler.DerivesOpenGeneric(typeof(EventHandler{})).</c>
    /// </example>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <param name="type">The type to dest derivation from.</param>
    /// <param name="openGeneric">An open generic type to be tested derivation against.</param>
    /// <returns>True if the type derives from the specified open generic, otherwise false.</returns>
    public static bool DerivesOpenGeneric(this Type type, Type openGeneric)
    {
        if (!openGeneric.IsGenericTypeDefinition)
        {
            return false;
        }

        return type.GetInterfaces()
                   .Where(static inter => inter.IsGenericType)
                   .Select(static inter => inter.GetGenericTypeDefinition())
                   .Any(inter => inter == openGeneric);
    }

    /// <summary>
    /// True if the
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsConcrete(this Type type)
    {
        return !type.IsInterface && !type.IsAbstract;
    }
}
