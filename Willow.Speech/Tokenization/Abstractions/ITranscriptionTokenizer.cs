using Willow.Speech.Tokenization.Models;

namespace Willow.Speech.Tokenization.Abstractions;

/// <summary>
/// Represents a mechanism to convert a word from the transcription into a token.
/// </summary>
/// <remarks>
/// This interface is detected automatically in assemblies registered, and added into the DI container, so any needed
/// dependencies can be loaded via the constructor.
/// </remarks>
public interface ITranscriptionTokenizer
{
    /// <summary>
    /// Processes the input string to locate the next valid token found within.
    /// </summary>
    /// <remarks>
    /// Currently the system sends only alphanumeric characters, after all the special characters were stripped. <br/>
    /// Implementors can not be certain it will always contain alphanumeric values if the need arises it will change.
    /// </remarks>
    /// <param name="input">The remaining string to be processed, after all previous processing were applied.</param>
    /// <returns>The result of the processing</returns>
    TokenProcessingResult Process(ReadOnlySpan<char> input);
}
