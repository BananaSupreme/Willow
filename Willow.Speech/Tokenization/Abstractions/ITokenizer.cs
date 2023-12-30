using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.Tokenization.Abstractions;

public interface ITokenizer
{
    Token[] Tokenize(string input);
}