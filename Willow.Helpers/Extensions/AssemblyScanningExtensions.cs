using System.Reflection;

namespace Willow.Helpers.Extensions;

public static class AssemblyScanningExtensions
{
    /// <summary>
    /// Finds all concrete types deriving <typeparamref name="T" /> in the assembly requested.
    /// </summary>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <param name="assembly">The assembly to search the types in.</param>
    /// <typeparam name="T">The type to look for concrete derivations of.</typeparam>
    /// <returns>Concrete types assignable to <typeparamref name="T" />.</returns>
    public static Type[] GetAllDeriving<T>(this Assembly assembly)
    {
        return assembly.GetAllDeriving(typeof(T));
    }

    /// <summary>
    /// Finds all concrete types deriving <paramref name="type" /> in the assembly requested.
    /// </summary>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <param name="assembly">The assembly to search the types in.</param>
    /// <param name="type">The type to look for concrete derivations of.</param>
    /// <returns>Concrete types assignable to <paramref name="type" />.</returns>
    public static Type[] GetAllDeriving(this Assembly assembly, Type type)
    {
        return assembly.GetTypes()
                       .Where(static t => t.IsConcrete() && !t.IsNestedPrivate)
                       .Where(t => t.IsAssignableTo(type))
                       .ToArray();
    }

    /// <summary>
    /// Finds all concrete types deriving the generic type definition <paramref name="openGeneric" /> in the assembly requested.
    /// </summary>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <param name="openGeneric">The type to look for concrete derivations of.</param>
    /// <param name="assembly">The assembly to search the types in.</param>
    /// <returns>Concrete types assignable to <paramref name="openGeneric" />.</returns>
    public static Type[] GetAllDerivingOpenGeneric(this Assembly assembly, Type openGeneric)
    {
        return assembly.GetTypes()
                       .Where(static t => t.IsConcrete() && !t.IsNestedPrivate)
                       .Where(t => t.DerivesOpenGeneric(openGeneric))
                       .ToArray();
    }
}
