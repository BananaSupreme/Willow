using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.Tokenization.Tokens;

public sealed record MergedToken(Token[] Tokens) : Token
{
    public override string GetString()
    {
        return string.Join(' ', Tokens.Select(static x => x.GetString()));
    }

    public override bool Match(Token other)
    {
        return other is MergedToken mergedToken && Match(mergedToken);
    }

    public bool Match(MergedToken other)
    {
        return Match(other.Tokens);
    }

    public bool Match(Token[] other)
    {
        return Match(other.AsMemory());
    }

    public bool Match(ReadOnlyMemory<Token> other)
    {
        if (other.Length != Tokens.Length)
        {
            return false;
        }

        for (var i = 0; i < other.Length; i++)
        {
            //Some tokens are more complex then others so we need to make sure, that the check is bidirectional,
            //For example word token to homophone might not match, but the homophone to the word will (because the
            //word was included as a homophone)
            if (!other.Span[i].Match(Tokens[i]) && !Tokens[i].Match(other.Span[i]))
            {
                return false;
            }
        }

        return true;
    }

    public bool Equals(MergedToken? other)
    {
        return other is not null && other.Tokens.SequenceEqual(Tokens);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Tokens);
    }
}
