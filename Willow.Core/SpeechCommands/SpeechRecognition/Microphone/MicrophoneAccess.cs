using Pv;

using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Models;
using Willow.Core.SpeechCommands.SpeechRecognition.Microphone.Settings;

namespace Willow.Core.SpeechCommands.SpeechRecognition.Microphone;

internal sealed class MicrophoneAccess : IMicrophoneAccess, IDisposable
{
    private const int _frameSize = 512;
    private readonly ILogger<MicrophoneAccess> _log;

    private readonly IOptionsMonitor<MicrophoneSettings> _options;

    private PvRecorder? _recorder;

    public MicrophoneAccess(IOptionsMonitor<MicrophoneSettings> options, ILogger<MicrophoneAccess> log)
    {
        _options = options;
        _log = log;
    }

    public void Dispose()
    {
        StopRecording();
        _recorder?.Dispose();
    }

    public IEnumerable<AudioData> StartRecording()
    {
        _recorder = PvRecorder.Create(_frameSize, _options.CurrentValue.MicrophoneIndex, 100);
        _recorder.Start();

        _log.RecordingStarted(_options.CurrentValue.MicrophoneIndex, _recorder.SelectedDevice,
            _options.CurrentValue.RecordingWindowTimeInMilliseconds);

        var recordingTime = _options.CurrentValue.RecordingWindowTimeInMilliseconds;
        var framesPerSecond = _recorder.SampleRate / _frameSize;
        var bufferSize = (int)Math.Ceiling(framesPerSecond * _frameSize * (recordingTime / 1000.0));
        var buffer = new short[bufferSize];

        while (_recorder.IsRecording)
        {
            yield return Record(bufferSize, buffer);
        }
    }

    private AudioData Record(int bufferSize, short[] buffer)
    {
        var framesProcessed = 0;
        var i = 0;
        while (i < bufferSize - _frameSize + 1
               && _recorder!.IsRecording)
        {
            var frame = _recorder.Read();
            frame.CopyTo(buffer, framesProcessed * _frameSize);
            framesProcessed++;
            i += _frameSize;
            Thread.Yield();
        }

        var data = new AudioData(buffer, _recorder!.SampleRate, 1, 16);
        _log.ReturningData(data);
        return data;
    }

    public void StopRecording()
    {
        _recorder?.Stop();
        _log.RecordingStopped();
    }
}

internal static partial class MicrophoneAccessLoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message =
            "Starting microphone recording. Microphone index: {microphoneIndex}, Microphone name: {microphoneName}, Recording window time: {recordingTime} ms.")]
    public static partial void RecordingStarted(this ILogger logger, int microphoneIndex, string microphoneName,
                                                int recordingTime);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Microphone recording stopped.")]
    public static partial void RecordingStopped(this ILogger logger);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Microphone return data. Data: {data}.")]
    public static partial void ReturningData(this ILogger logger, AudioData data);
}