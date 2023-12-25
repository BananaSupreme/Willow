using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Python.Included;

using Willow.Core.Helpers;
using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Models;
using Willow.Core.SpeechCommands.SpeechRecognition.SpeechToText.Abstractions;
using Willow.WhisperServer.Extensions;
using Willow.WhisperServer.Models;
using Willow.WhisperServer.Settings;

namespace Willow.WhisperServer;

internal sealed class WhisperEngine : IDisposable, IAsyncDisposable, ISpeechToTextEngine, IHostedService
{
    private bool _isRunning;
    private bool _disposed;
    private bool _firstLoadDone;
    private readonly DisposableLock _lock = new();
    private readonly ILogger<WhisperEngine> _log;
    private readonly IOptionsMonitor<WhisperModelSettings> _modelSettingsMonitor;
    private readonly IOptionsMonitor<TranscriptionSettings> _transcriptionSettingsMonitor;
    private PyModule? _scope;
    private nint _state;

    public WhisperEngine(IOptionsMonitor<WhisperModelSettings> modelSettingsMonitor,
                         IOptionsMonitor<TranscriptionSettings> transcriptionSettingsMonitor,
                         ILogger<WhisperEngine> log)
    {
        _modelSettingsMonitor = modelSettingsMonitor;
        _transcriptionSettingsMonitor = transcriptionSettingsMonitor;
        _log = log;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var _ = await _lock.LockAsync();

        EnsureNotDisposed();

        if (_isRunning)
        {
            return;
        }

        await InitPythonDependenciesAsync();
        InitPythonEngine();
        InitModule();
        InitializeModel();
        _modelSettingsMonitor.OnChange(_ =>
        {
            _log.ModelReinitializing();
            InitializeModel();
        });

        _isRunning = true;
    }

    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var locker = await _lock.LockAsync();

        if (!_isRunning)
        {
            return;
        }

        using var gil = Py.GIL();
        _scope?.Dispose();
        RuntimeData.ClearStash();
        _ = Runtime.TryCollectingGarbage(5);

        _isRunning = false;
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
        var modelSettings = _modelSettingsMonitor.CurrentValue;
        _log.ModelInitializing(modelSettings);
        _scope?.InitializeWhisperModel(modelSettings);
        _log.ModelInitialized();
    }

    public async Task<string> TranscribeAudioAsync(AudioData audioData)
    {
        EnsureNotDisposed();
        var result = await Task.Run(() => Transcribe(new(audioData)));
        return result;
    }

    private string Transcribe(TranscriptionParameters transcriptionParameters)
    {
        using var gil = Py.GIL();
        var transcriptionSettings = _transcriptionSettingsMonitor.CurrentValue;
        _log.TranscriptionRequested(transcriptionSettings, transcriptionParameters);
        var transcription = _scope?.TranscribeAudio(transcriptionParameters, transcriptionSettings)
                            ?? throw new InvalidOperationException("Transcribe was called without the start method");
        _log.AudioTranscribed();
        _log.TranscriptionDetails(transcription);
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

}

internal static partial class WhisperEngineLoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Transcribed Audio.")]
    public static partial void AudioTranscribed(this ILogger log);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "Transcribed Audio: ({transcription})")]
    public static partial void TranscriptionDetails(this ILogger log, string transcription);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Transcription Requested: ({transcriptionSettings}, {transcriptionParameters})")]
    public static partial void TranscriptionRequested(this ILogger log, TranscriptionSettings transcriptionSettings,
                                                      TranscriptionParameters transcriptionParameters);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "Model initializing: ({modelSettings})")]
    public static partial void ModelInitializing(this ILogger log, WhisperModelSettings modelSettings);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Model initialized")]
    public static partial void ModelInitialized(this ILogger log);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Trace,
        Message = "Settings changed,model re-initialization requested")]
    public static partial void ModelReinitializing(this ILogger log);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Trace,
        Message = "Whisper server stopping")]
    public static partial void ServerStopping(this ILogger log);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Trace,
        Message = "Whisper server stopped")]
    public static partial void ServerStopped(this ILogger log);
}