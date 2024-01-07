using System.Text;

namespace Willow.Helpers.Extensions;

internal static class StringExtensions
{
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