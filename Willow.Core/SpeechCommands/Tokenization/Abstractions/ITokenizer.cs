using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;

namespace Willow.Core.SpeechCommands.Tokenization.Abstractions;

public interface ITokenizer
{
    Token[] Tokenize(string input);
}