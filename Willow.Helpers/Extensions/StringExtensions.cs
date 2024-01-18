using System.Text;

namespace Willow.Helpers.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Converts a pascal cased string to title cased.
    /// </summary>
    /// <example>
    /// <c>ThisIsPascal -> This Is Pascal</c>
    /// </example>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <param name="span">Input string as pascal case.</param>
    /// <returns>The input string as a title case.</returns>
    public static ReadOnlySpan<char> GetTitleFromPascal(this ReadOnlySpan<char> span)
    {
        StringBuilder stringBuilder = new();
        stringBuilder.Append(span[0]);

        for (var i = 1; i < span.Length; i++)
        {
            if (char.IsUpper(span[i]))
            {
                stringBuilder.Append(' ');
            }

            stringBuilder.Append(span[i]);
        }

        return stringBuilder.ToString().AsSpan();
    }
}
