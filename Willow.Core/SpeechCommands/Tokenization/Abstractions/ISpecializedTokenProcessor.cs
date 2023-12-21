using Willow.Core.SpeechCommands.Tokenization.Models;
using Willow.Core.SpeechCommands.Tokenization.Tokens;

namespace Willow.Core.SpeechCommands.Tokenization.Abstractions;

public interface ISpecializedTokenProcessor
{
    TokenProcessingResult Process(ReadOnlySpan<char> input);

    static TokenProcessingResult Fail()
    {
        return new(false, new EmptyToken(), 0);
    }
}