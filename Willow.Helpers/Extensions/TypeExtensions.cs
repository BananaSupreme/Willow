namespace Willow.Helpers.Extensions;

public static class TypeExtensions
{
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
