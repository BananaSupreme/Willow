using Willow.Speech.Tokenization.Tokens.Abstractions;

using Phonix;

using Willow.Speech.Tokenization.Enums;

namespace Willow.Speech.Tokenization.Tokens;

/// <summary>
/// A token representing a word that is equal whenever its underlying code is equal, we allow Caverphone and Metaphone
/// for now.
/// </summary>
/// <seealso href="https://en.wikipedia.org/wiki/Caverphone"/>
/// <seealso href="https://en.wikipedia.org/wiki/Metaphone"/>
internal sealed record EncodingToken(string Value, WordEncoderType WordEncoderType) : WordToken(Value)
{
    private static readonly DoubleMetaphone _metaphone = new();
    private static readonly CaverPhone _caverPhone = new();

    public string? Key { get; } = BuildKeySafe(Value, WordEncoderType);

    public override bool Match(Token other)
    {
        return other switch
        {
            EncodingToken metaphone => metaphone.Key == Key,
            WordToken word => Match(word),
            _ => base.Match(other)
        };
    }

    private bool Match(WordToken other)
    {
        if (other.Value == Value)
        {
            return true;
        }

        var inputKey = BuildKeySafe(other.Value, WordEncoderType);
        return inputKey == Key;
    }

    private static string BuildKeySafe(string input, WordEncoderType encoderType)
    {
        try
        {
            return encoderType switch
            {
                WordEncoderType.Metaphone => _metaphone.BuildKey(input),
                WordEncoderType.Caverphone => _caverPhone.BuildKey(input)
            };
        }
        //This library is a bit broken so it throws sometimes, maybe we can fix it some day, but not today
        catch (Exception)
        {
            return input;
        }
    }

    public bool Equals(EncodingToken? other)
    {
        return base.Equals(other)
               && string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase)
               && WordEncoderType == other.WordEncoderType;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Key, WordEncoderType);
    }
}
