using Microsoft.Extensions.Hosting;

using System.Text.Json;
using System.Text.Json.Serialization;

using Vosk;

using Willow.Helpers.Locking;
using Willow.Speech.Microphone.Models;
using Willow.Speech.SpeechToText.Abstractions;

namespace Willow.Vosk;

internal sealed class VoskEngine : ISpeechToTextEngine, IHostedService, IDisposable, IAsyncDisposable
{
    private readonly DisposableLock _lock = new();
    private Model _model = null!;
    private VoskRecognizer _recognizer = null!;

    public async Task<string> TranscribeAudioAsync(AudioData audioData)
    {
        using var __ = await _lock.LockAsync();
        var resultJson = await Task.Run(() =>
        {
            _ = _recognizer.AcceptWaveform(audioData.RawData, audioData.RawData.Length);
            var recognizerResult = _recognizer.FinalResult();
            _recognizer.Reset();
            return recognizerResult;
        });
        var result = JsonSerializer.Deserialize<VoskResult>(resultJson);
        return result.Text;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var __ = await _lock.LockAsync();
        await Task.Run(() =>
        {
            _model = new("./model");
            _recognizer = new(_model, 16000);
        }, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var __ = await _lock.LockAsync();
        _model.Dispose();
        _recognizer.Dispose();
    }

    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None);
    }

    private record struct VoskResult([property:JsonPropertyName("text")] string Text);
}