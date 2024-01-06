using Willow.Core.Eventing.Abstractions;
using Willow.Helpers;
using Willow.Speech.SpeechToText.Eventing.Events;

namespace Willow.Speech.Tokenization.Eventing.Interceptors;

internal sealed class PunctuationRemoverInterceptor : IEventInterceptor<AudioTranscribedEvent>
{
    public async Task InterceptAsync(AudioTranscribedEvent @event, Func<AudioTranscribedEvent, Task> next)
    {
        var textProcessing = @event.Text.Where(x => CachedSearchValues.AlphanumericAndSpaces.Contains(x)).ToArray();
        var newText = new string(textProcessing);
        await next(new(newText));
    }
}