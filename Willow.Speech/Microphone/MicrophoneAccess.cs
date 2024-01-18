using System.Diagnostics.CodeAnalysis;

using Pv;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.Settings.Abstractions;
using Willow.Core.Settings.Events;
using Willow.Helpers.Locking;
using Willow.Speech.Microphone.Abstractions;
using Willow.Speech.Microphone.Models;
using Willow.Speech.Microphone.Settings;

namespace Willow.Speech.Microphone;

internal sealed class MicrophoneAccess
    : IMicrophoneAccess, IDisposable, IEventHandler<SettingsUpdatedEvent<MicrophoneSettings>>
{
    private const int FrameSize = 512;
    private static readonly DisposableLock _lock = new();
    private readonly ILogger<MicrophoneAccess> _log;

    private readonly ISettings<MicrophoneSettings> _microphoneSettings;
    private short[]? _buffer;

    private PvRecorder? _recorder;

    public MicrophoneAccess(ISettings<MicrophoneSettings> microphoneSettings, ILogger<MicrophoneAccess> log)
    {
        _microphoneSettings = microphoneSettings;
        _log = log;
    }

    public void Dispose()
    {
        StopRecording();
        _lock.Dispose();
    }

    public async Task HandleAsync(SettingsUpdatedEvent<MicrophoneSettings> @event)
    {
        using var _ = await _lock.LockAsync();
        StopRecording();
        SetupMicrophone();
    }

    public IEnumerable<AudioData> StartRecording()
    {
        SetupMicrophone();

        //When changing the recorder sometimes the isRecording value gets captured as false since we have to
        //stop and start a new recorded, so this holds the value while the change happens making sure we wait for the lock
        var isRecording = _recorder.IsRecording;
        while (isRecording)
        {
            var @lock = _lock.Lock();
            AudioData data;
            try
            {
                data = Record(_recorder, _buffer.Length, _buffer);
                isRecording = _recorder.IsRecording;
            }
            finally
            {
                @lock.Dispose();
            }

            _log.ReturningData(data);
            yield return data;
        }
    }

    public void StopRecording()
    {
        _recorder?.Stop();
        _recorder?.Dispose();
        _log.RecordingStopped();
    }

    [MemberNotNull(nameof(_buffer), nameof(_recorder))]
    private void SetupMicrophone()
    {
        _recorder = PvRecorder.Create(FrameSize, _microphoneSettings.CurrentValue.MicrophoneIndex, 100);
        _recorder.Start();
        _log.RecordingStarted(_microphoneSettings.CurrentValue.MicrophoneIndex,
                              _recorder.SelectedDevice,
                              _microphoneSettings.CurrentValue.RecordingWindowTimeInMilliseconds);

        var recordingTime = _microphoneSettings.CurrentValue.RecordingWindowTimeInMilliseconds;
        var framesPerSecond = _recorder.SampleRate / FrameSize;
        var bufferSize = (int)Math.Ceiling(framesPerSecond * FrameSize * (recordingTime / 1000.0));
        _buffer = new short[bufferSize];
    }

    private static AudioData Record(PvRecorder recorder, int bufferSize, short[] buffer)
    {
        var framesProcessed = 0;
        var i = 0;
        while (i < bufferSize - FrameSize + 1 && recorder.IsRecording)
        {
            var frame = recorder.Read();
            frame.CopyTo(buffer, framesProcessed * FrameSize);
            framesProcessed++;
            i += FrameSize;
            Thread.Yield();
        }

        var data = new AudioData(buffer, recorder.SampleRate, 1, 16);
        return data;
    }
}

internal static partial class MicrophoneAccessLoggingExtensions
{
    [LoggerMessage(EventId = 1,
                   Level = LogLevel.Information,
                   Message
                       = "Starting microphone recording. Microphone index: {microphoneIndex}, Microphone name: {microphoneName}, Recording window time: {recordingTime} ms.")]
    public static partial void RecordingStarted(this ILogger logger,
                                                int microphoneIndex,
                                                string microphoneName,
                                                int recordingTime);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Microphone recording stopped.")]
    public static partial void RecordingStopped(this ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Microphone return data. Data: {data}.")]
    public static partial void ReturningData(this ILogger logger, AudioData data);
}
