using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Models;
using Willow.Core.SpeechCommands.SpeechRecognition.SpeechToText.Abstractions;
using Willow.WhisperServer.Extensions;
using Willow.WhisperServer.Models;
using Willow.WhisperServer.Settings;

namespace Willow.WhisperServer;

public sealed class WhisperEngine : IDisposable, ISpeechToTextEngine
{
    private readonly ILogger<WhisperEngine> _log;
    private readonly IOptionsMonitor<WhisperModelSettings> _modelSettingsMonitor;
    private readonly PyModule _scope;
    private readonly nint _state;
    private readonly IOptionsMonitor<TranscriptionSettings> _transcriptionSettingsMonitor;

    public WhisperEngine(IOptionsMonitor<WhisperModelSettings> modelSettingsMonitor,
                         IOptionsMonitor<TranscriptionSettings> transcriptionSettingsMonitor,
                         IOptions<PythonSettings> pythonSettings,
                         ILogger<WhisperEngine> log)
    {
        _modelSettingsMonitor = modelSettingsMonitor;
        _transcriptionSettingsMonitor = transcriptionSettingsMonitor;
        _log = log;

        Runtime.PythonDLL = pythonSettings.Value.PathToPythonDll;
        PythonEngine.Initialize();
        _state = PythonEngine.BeginAllowThreads();
        using var gil = Py.GIL();
        _scope = Py.CreateScope();
        Init();
    }

    public void Dispose()
    {
        _log.DisposingStarted();
        PythonEngine.Shutdown();
        PythonEngine.EndAllowThreads(_state);
        _scope.Dispose();
        _log.DisposingEnded();
    }

    public async Task<string> TranscribeAudioAsync(AudioData audioData)
    {
        var result = await Task.Run(() => Transcribe(new(audioData)));
        return result;
    }

    private void Init()
    {
        _scope.Exec(
            """
            from faster_whisper import WhisperModel
            import io
            """);

        InitializeModel();
        _modelSettingsMonitor.OnChange(_ =>
        {
            _log.ModelReinitializing();
            InitializeModel();
        });
    }

    private void InitializeModel()
    {
        using var gil = Py.GIL();
        var modelSettings = _modelSettingsMonitor.CurrentValue;
        _log.ModelInitializing(modelSettings);
        _scope.InitializeWhisperModel(modelSettings);
        _log.ModelInitialized();
    }

    public string Transcribe(TranscriptionParameters transcriptionParameters)
    {
        using var gil = Py.GIL();
        var transcriptionSettings = _transcriptionSettingsMonitor.CurrentValue;
        _log.TranscriptionRequested(transcriptionSettings, transcriptionParameters);
        var transcription = _scope.TranscribeAudio(transcriptionParameters, transcriptionSettings);
        _log.AudioTranscribed();
        _log.TranscriptionDetails(transcription);
        return transcription;
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
        Message = "Disposing Started")]
    public static partial void DisposingStarted(this ILogger log);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Trace,
        Message = "Disposing Ended")]
    public static partial void DisposingEnded(this ILogger log);
}