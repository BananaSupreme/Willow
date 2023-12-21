using System.Reflection;

namespace Willow.Core.Helpers.Extensions;

internal static class TypeExtensions
{
    public static string GetFullName<T>()
    {
        return GetFullName(typeof(T));
    }

    public static string GetFullName(Type t)
    {
        return t.FullName ?? t.Name;
    }

    public static Type[] GetAllDerivingInOwnAssembly(this Type type)
    {
        return type.GetAllDerivingInAssembly(type.Assembly);
    }

    public static Type[] GetAllDerivingInAssembly(this Type type, Assembly assembly)
    {
        return assembly.GetTypes()
                       .Where(t => t.IsConcrete())
                       .Where(t => t.IsAssignableTo(type))
                       .ToArray();
    }

    public static bool DerivesOpenGeneric(this Type type, Type openGeneric)
    {
        if (!openGeneric.IsGenericTypeDefinition)
        {
            return false;
        }

        return type.GetInterfaces()
                   .Where(inter => inter.IsGenericType)
                   .Select(inter => inter.GetGenericTypeDefinition())
                   .Any(inter => inter == openGeneric);
    }
    
    public static bool IsConcrete(this Type type)
    {
        return !type.IsInterface && !type.IsAbstract;
    }
}