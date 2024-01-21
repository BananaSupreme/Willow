using Willow.Core.Middleware.Abstractions;
using Willow.Helpers;
using Willow.Speech.SpeechToText.Eventing.Events;

namespace Willow.Speech.Tokenization.Middleware;

/// <summary>
/// Intercepts the <see cref="AudioTranscribedEvent" /> to remove any symbol that is not alphanumeric or space.
/// </summary>
internal sealed class PunctuationRemoverMiddleware : IMiddleware<AudioTranscribedEvent>
{
    public async Task ExecuteAsync(AudioTranscribedEvent input, Func<AudioTranscribedEvent, Task> next)
    {
        var textProcessing = input.Text.Where(static x => CachedSearchValues.AlphanumericAndSpaces.Contains(x))
                                  .ToArray();
        var newText = new string(textProcessing);
        await next(new AudioTranscribedEvent(newText));
    }
}
