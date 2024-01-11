using Microsoft.Extensions.Logging;

using System.Text.Json;
using System.Text.Json.Serialization;

using Vosk;

using Willow.Core.Environment.Enums;
using Willow.Core.Eventing.Abstractions;
using Willow.Core.Privacy.Settings;
using Willow.Core.Settings.Abstractions;
using Willow.Core.Settings.Events;
using Willow.Helpers.Locking;
using Willow.Helpers.Logging.Loggers;
using Willow.Speech.Microphone.Models;
using Willow.Speech.SpeechToText.Abstractions;
using Willow.Speech.SpeechToText.Enums;
using Willow.Vosk.Abstractions;
using Willow.Vosk.Settings;

namespace Willow.Vosk;

internal sealed class VoskEngine : 
    ISpeechToTextEngine, 
    IDisposable, 
    IAsyncDisposable,
    IEventHandler<SettingsUpdatedEvent<VoskSettings>>
{
    private readonly IVoskModelInstaller _modelInstaller;
    private readonly ISettings<VoskSettings> _voskSettings;
    private readonly ISettings<PrivacySettings> _privacySettings;
    private readonly ILogger<VoskEngine> _log;
    private readonly DisposableLock _lock = new();
    private Model _model = null!;
    private VoskRecognizer _recognizer = null!;

    public VoskEngine(IVoskModelInstaller modelInstaller,
                      ISettings<VoskSettings> voskSettings,
                      ISettings<PrivacySettings> privacySettings,
                      ILogger<VoskEngine> log)
    {
        _modelInstaller = modelInstaller;
        _voskSettings = voskSettings;
        _privacySettings = privacySettings;
        _log = log;
    }

    public string Name => nameof(SelectedSpeechEngine.Vosk);
    public SupportedOperatingSystems SupportedOperatingSystems => SupportedOperatingSystems.Windows;
    public bool IsRunning { get; private set; }

    public async Task<string> TranscribeAudioAsync(AudioData audioData)
    {
        using var __ = await _lock.LockAsync();
        var resultJson = await Task.Run(() =>
        {
            _log.TranscriptionRequested(audioData);
            _ = _recognizer.AcceptWaveform(audioData.RawData, audioData.RawData.Length);
            var recognizerResult = _recognizer.FinalResult();
            _recognizer.Reset();
            _log.AudioTranscribed(new(recognizerResult, _privacySettings.CurrentValue.AllowLoggingTranscriptions));
            return recognizerResult;
        });
        var result = JsonSerializer.Deserialize<VoskResult>(resultJson);
        return result.Text;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        using var __ = await _lock.LockAsync();
        _log.StartingVosk();
        IsRunning = true;
        await _modelInstaller.EnsureExistsAsync();
        await Task.Run(() =>
        {
            _model = new(_voskSettings.CurrentValue.ModelPath);
            _recognizer = new(_model, 16000);
            _log.VoskStarted();
        }, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        using var __ = await _lock.LockAsync();
        _log.StoppingVosk();
        IsRunning = false;
        _model.Dispose();
        _recognizer.Dispose();
        _log.VoskStopped();
    }
    
    public async Task HandleAsync(SettingsUpdatedEvent<VoskSettings> @event)
    {
        if (!IsRunning)
        {
            return;
        }
        _log.ModelReinitializing();
        await StopAsync();
        await StartAsync();
    }

    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None);
    }

    private record struct VoskResult([property: JsonPropertyName("text")] string Text);
}

internal static partial class VoskEngineLoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Transcribed Audio: ({transcription})")]
    public static partial void AudioTranscribed(this ILogger log, RedactingLogger<string> transcription);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "Transcription Requested: ({data})")]
    public static partial void TranscriptionRequested(this ILogger log, AudioData data);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Settings changed, Vosk model re-initialization requested")]
    public static partial void ModelReinitializing(this ILogger log);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Trace,
        Message = "Vosk server starting")]
    public static partial void StartingVosk(this ILogger log);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Trace,
        Message = "Vosk server started")]
    public static partial void VoskStarted(this ILogger log);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Trace,
        Message = "Vosk server stopping")]
    public static partial void StoppingVosk(this ILogger log);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Trace,
        Message = "Vosk server stopped")]
    public static partial void VoskStopped(this ILogger log);
}