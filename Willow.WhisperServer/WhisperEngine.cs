using Microsoft.Extensions.Logging;

using Python.Included;

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
using Willow.WhisperServer.Extensions;
using Willow.WhisperServer.Models;
using Willow.WhisperServer.Settings;

namespace Willow.WhisperServer;

/// <summary>
/// Open-AI whisper engine, under the scenes creates dynamically runs python to call the `faster-whisper` package.
/// </summary>
/// <remarks>
/// This engine manages the python runtime, if another implementor will try to create a package that calls into python
/// they will run into trouble, for now we do not support multiple languages but this will be in an issue if we do.
/// </remarks>
/// <seealso href="https://github.com/SYSTRAN/faster-whisper/tree/master"/>
internal sealed class WhisperEngine : 
    IDisposable, 
    IAsyncDisposable, 
    ISpeechToTextEngine, 
    IEventHandler<SettingsUpdatedEvent<WhisperModelSettings>>
{
    private bool _disposed;
    private bool _firstLoadDone;
    private readonly DisposableLock _lock = new();
    private readonly ILogger<WhisperEngine> _log;
    private readonly ISettings<WhisperModelSettings> _modelSettings;
    private readonly ISettings<TranscriptionSettings> _transcriptionSettings;
    private readonly ISettings<PrivacySettings> _privacySettings;
    private PyModule? _scope;
    private nint _state;

    public string Name => nameof(SelectedSpeechEngine.Whisper);
    public SupportedOss SupportedOss => SupportedOss.Windows;
    public bool IsRunning { get; private set; }
    
    public WhisperEngine(ISettings<WhisperModelSettings> modelSettings,
                         ISettings<TranscriptionSettings> transcriptionSettings,
                         ISettings<PrivacySettings> privacySettings,
                         ILogger<WhisperEngine> log)
    {
        _modelSettings = modelSettings;
        _transcriptionSettings = transcriptionSettings;
        _privacySettings = privacySettings;
        _log = log;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        using var _ = await _lock.LockAsync();
        _log.StartingWhisper();
        EnsureNotDisposed();

        if (IsRunning)
        {
            return;
        }

        await InitPythonDependenciesAsync();
        InitPythonEngine();
        InitModule();
        InitializeModel();
        _log.WhisperStarted();
        IsRunning = true;
    }

    
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        using var locker = await _lock.LockAsync();
        _log.StoppingWhisper();
        if (!IsRunning)
        {
            return;
        }

        using var gil = Py.GIL();
        _scope?.Exec(
            """
            model = None
            """);
        _scope?.Dispose();
        RuntimeData.ClearStash();
        _ = Runtime.TryCollectingGarbage(5);
        _log.WhisperStopped();
        IsRunning = false;
    }

    private async Task InitPythonDependenciesAsync()
    {
        await Installer.SetupPython();
        await Installer.TryInstallPip();
        await Installer.PipInstallModule("faster_whisper", "0.9.0");
    }

    private void InitPythonEngine()
    {
        if (_firstLoadDone)
        {
            return;
        }

        PythonEngine.Initialize();
        _state = PythonEngine.BeginAllowThreads();
        _firstLoadDone = true;
    }

    private void InitModule()
    {
        using var gil = Py.GIL();
        _scope = Py.CreateScope();
        _scope?.Exec(
            """
            from faster_whisper import WhisperModel
            import io
            """);
    }

    private void InitializeModel()
    {
        using var gil = Py.GIL();
        var modelSettings = _modelSettings.CurrentValue;
        _log.ModelInitializing(modelSettings);
        _scope?.InitializeWhisperModel(modelSettings);
        _log.ModelInitialized(modelSettings);
    }

    public async Task<string> TranscribeAudioAsync(AudioData audioData)
    {
        EnsureNotDisposed();
        if (!IsRunning)
        {
            return string.Empty;
        }
        
        var result = await Task.Run(() => Transcribe(new(audioData)));
        return result;
    }

    private string Transcribe(TranscriptionParameters transcriptionParameters)
    {
        using var gil = Py.GIL();
        var transcriptionSettings = _transcriptionSettings.CurrentValue;
        _log.TranscriptionRequested(transcriptionSettings, transcriptionParameters);
        var transcription = _scope?.TranscribeAudio(transcriptionParameters, transcriptionSettings)
                            ?? throw new InvalidOperationException("Transcribe was called without the start method");
        _log.AudioTranscribed(new(transcription, _privacySettings.CurrentValue.AllowLoggingTranscriptions));
        return transcription;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await StopAsync(CancellationToken.None);
        PythonEngine.Shutdown();
        PythonEngine.EndAllowThreads(_state);
    }

    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }
    
    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(WhisperEngine));
        }
    }

    public Task HandleAsync(SettingsUpdatedEvent<WhisperModelSettings> @event)
    {
        _log.ModelReinitializing();
        InitializeModel();
        return Task.CompletedTask;
    }
}

internal static partial class WhisperEngineLoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Transcribed Audio: ({transcription})")]
    public static partial void AudioTranscribed(this ILogger log, RedactingLogger<string> transcription);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "Transcription Requested: ({transcriptionSettings}, {transcriptionParameters})")]
    public static partial void TranscriptionRequested(this ILogger log, TranscriptionSettings transcriptionSettings,
                                                      TranscriptionParameters transcriptionParameters);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Model initializing: ({modelSettings})")]
    public static partial void ModelInitializing(this ILogger log, WhisperModelSettings modelSettings);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Model initialized: ({modelSettings})")]
    public static partial void ModelInitialized(this ILogger log, WhisperModelSettings modelSettings);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Debug,
        Message = "Settings changed, Whisper model re-initialization requested")]
    public static partial void ModelReinitializing(this ILogger log);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Trace,
        Message = "Whisper server starting")]
    public static partial void StartingWhisper(this ILogger log);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Trace,
        Message = "Whisper server started")]
    public static partial void WhisperStarted(this ILogger log);
    
    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Trace,
        Message = "Whisper server stopping")]
    public static partial void StoppingWhisper(this ILogger log);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Trace,
        Message = "Whisper server stopped")]
    public static partial void WhisperStopped(this ILogger log);
}