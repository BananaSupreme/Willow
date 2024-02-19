namespace Willow.Helpers.Extensions;

public static class GuardExtensions
{
    public static void ThrowIfAnyNull<T>(this IEnumerable<T>? input)
    {
        ArgumentNullException.ThrowIfNull(input);
        foreach (var item in input)
        {
            ArgumentNullException.ThrowIfNull(item);
        }
    }
}
