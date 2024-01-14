using System.Diagnostics;

using Willow.Core.Eventing.Abstractions;
using Willow.Speech.Microphone.Eventing.Events;
using Willow.Speech.Microphone.Extensions;

namespace Willow.Speech.Microphone.Eventing.Interceptors;

/// <summary>
/// Intercepts the <see cref="AudioCapturedEvent"/> to create WAV files, only works in debug mode.
/// </summary>
internal sealed class DebuggingVoiceWavOutputInterceptor : IEventInterceptor<AudioCapturedEvent>
{
    private const string _folderName = "./wavFiles";

    public DebuggingVoiceWavOutputInterceptor()
    {
        Directory.CreateDirectory(_folderName);
        DeleteOldFiles();
    }

    public async Task InterceptAsync(AudioCapturedEvent @event, Func<AudioCapturedEvent, Task> next)
    {
        var timeStamp = TimeSpan.FromTicks(Stopwatch.GetTimestamp());
        var fileName = $"recording - {timeStamp:hh\\.mm\\.fffffff}_{Guid.NewGuid()}.wav";
        fileName = Path.Combine(_folderName, fileName);
        await using (var file = File.Create(fileName))
        {
            await file.WriteAsync(@event.AudioData.ToWavFile());
            await file.FlushAsync();
        }

        await next(@event);
    }

    private static void DeleteOldFiles()
    {
        var oldFiles = Directory.GetFiles(_folderName);
        foreach (var file in oldFiles)
        {
            File.Delete(file);
        }
    }
}