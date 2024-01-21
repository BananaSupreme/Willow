using System.Diagnostics;

using Willow.Core.Middleware.Abstractions;
using Willow.Speech.Microphone.Eventing.Events;
using Willow.Speech.Microphone.Extensions;

namespace Willow.Speech.Microphone.Middleware;

/// <summary>
/// Intercepts the <see cref="AudioCapturedEvent" /> to create WAV files, only works in debug mode.
/// </summary>
internal sealed class DebuggingVoiceWavOutputMiddleware : IMiddleware<AudioCapturedEvent>
{
    private const string FolderName = "./wavFiles";

    public DebuggingVoiceWavOutputMiddleware()
    {
        Directory.CreateDirectory(FolderName);
        DeleteOldFiles();
    }

    public async Task ExecuteAsync(AudioCapturedEvent input, Func<AudioCapturedEvent, Task> next)
    {
        var timeStamp = TimeSpan.FromTicks(Stopwatch.GetTimestamp());
        var fileName = $@"recording - {timeStamp:hh\.mm\.fffffff}_{Guid.NewGuid()}.wav";
        fileName = Path.Combine(FolderName, fileName);
        await using (var file = File.Create(fileName))
        {
            await file.WriteAsync(input.AudioData.ToWavFile());
            await file.FlushAsync();
        }

        await next(input);
    }

    private static void DeleteOldFiles()
    {
        var oldFiles = Directory.GetFiles(FolderName);
        foreach (var file in oldFiles)
        {
            File.Delete(file);
        }
    }
}
