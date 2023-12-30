using Willow.Speech.Tokenization.Models;
using Willow.Speech.Tokenization.Tokens;

namespace Willow.Speech.Tokenization.Abstractions;

public interface ISpecializedTokenProcessor
{
    TokenProcessingResult Process(ReadOnlySpan<char> input);

    static TokenProcessingResult Fail()
    {
        return new(false, new EmptyToken(), 0);
    }
}