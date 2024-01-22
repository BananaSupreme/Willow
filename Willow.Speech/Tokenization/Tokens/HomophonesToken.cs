using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.Tokenization.Tokens;

/// <summary>
/// Allows to include a token and homophones of it.
/// </summary>
/// <param name="Internal"></param>
/// <param name="Homophones"></param>
public sealed record HomophonesToken(WordToken Internal, string[] Homophones) : WordToken(Internal.Value)
{
    public override string GetString()
    {
        return Internal.GetString();
    }

    public override int GetInt32()
    {
        return Internal.GetInt32();
    }

    public override bool Match(Token other)
    {
        return other switch
        {
            WordToken word => Internal.Match(word) || Homophones.Contains(word.GetString()),
            _ => Internal.Match(other)
        };
    }

    public override string ToString()
    {
        return $"Internal: {Value}, Homophones: {string.Join(' ', Homophones)}";
    }
}
