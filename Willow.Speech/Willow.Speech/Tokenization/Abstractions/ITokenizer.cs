using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Willow.Speech.Tokenization.Abstractions;

/// <summary>
/// Manages transforming an input into the tokens to be parsed by the system.
/// </summary>
internal interface ITokenizer
{
    /// <inheritdoc cref="ITokenizer" />
    /// <param name="input">The input string to tokenize.</param>
    /// <returns>A collection of tokens representing the input string.</returns>
    Token[] Tokenize(string input);
}
