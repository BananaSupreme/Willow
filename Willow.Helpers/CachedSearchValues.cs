using System.Buffers;
// ReSharper disable StringLiteralTypo

namespace Willow.Helpers;

/// <summary>
/// A cache of <see cref="SearchValues" />.
/// </summary>
/// <remarks>
/// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
/// IT WITHOUT NOTICE!
/// </remarks>
public static class CachedSearchValues
{
    public static readonly SearchValues<char> Alphanumeric
        = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");

    public static readonly SearchValues<char> CapitalAlphabet = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

    public static readonly SearchValues<char> AlphanumericAndSpaces
        = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ");

    public static readonly SearchValues<char> ValidVariableCharacters
        = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_@");

    public static readonly SearchValues<char> ValidVariableStarters
        = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_@");

    public static readonly SearchValues<char> Alphabet
        = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
    
    public static readonly SearchValues<char> AlphabetAndSpaces
        = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ");
}
