using System.Text;
using System.Threading.Channels;

using Willow.Registration;
using Willow.Settings.Abstractions;
using Willow.Settings.Models;

namespace Willow.Settings;

internal sealed class FileWritingWorker : IBackgroundWorker, IQueuedFileWriter
{
    private readonly byte[] _buffer = new byte[512];
    private readonly Channel<FileUpdateRequest> _channel = Channel.CreateUnbounded<FileUpdateRequest>();
    private readonly ILogger<FileWritingWorker> _log;

    public FileWritingWorker(ILogger<FileWritingWorker> log)
    {
        _log = log;
    }

    public void QueueRequest(FileUpdateRequest request)
    {
        _log.RequestQueued(request);
        _channel.Writer.TryWrite(request);
    }

    public void Flush()
    {
        while (_channel.Reader.TryRead(out var request))
        {
            ProcessUpdateAsync(request).GetAwaiter().GetResult();
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await AcceptFilesAsync(cancellationToken);
    }

    public Task StopAsync()
    {
        Flush();
        return Task.CompletedTask;
    }

    private async Task AcceptFilesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var request in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await ProcessUpdateAsync(request);
            }
        }
        catch (OperationCanceledException)
        {
            //nothing to do here
        }
    }

    private async Task ProcessUpdateAsync(FileUpdateRequest request)
    {
        var attempt = 0;
        while (attempt < 10)
        {
            _log.ProcessingRequest(request);
            try
            {
                await ProcessUpdateAttemptAsync(request);
                break;
            }
            catch (Exception ex)
            {
                _log.FileWritingFailed(request, attempt, ex);
            }

            await Task.Delay((int)Math.Pow(100, attempt), CancellationToken.None);
            attempt++;
        }
    }

    private async Task ProcessUpdateAttemptAsync(FileUpdateRequest request)
    {
        request.File.Seek(0, SeekOrigin.Begin);
        if (request.Value.Length < _buffer.Length)
        {
            _log.RequestFitsBuffer();
            Encoding.UTF8.GetBytes(request.Value.AsSpan(), _buffer.AsSpan());
            var buffer = _buffer.AsMemory()[..request.Value.Length];
            await request.File.WriteAsync(buffer, CancellationToken.None);
            await request.File.FlushAsync(CancellationToken.None);
            _buffer.AsSpan().Clear();
            return;
        }

        _log.RequestExceededBuffer();
        var bytes = Encoding.UTF8.GetBytes(request.Value);
        await request.File.WriteAsync(bytes, CancellationToken.None); //This should finish and not throw in the middle
    }
}

internal static partial class FileWritingWorkerLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Requested file write ({request}) processing now")]
    public static partial void ProcessingRequest(this ILogger logger, FileUpdateRequest request);

    [LoggerMessage(EventId = 2,
                   Level = LogLevel.Error,
                   Message = "File writing failed ({request}), attempt ({attempt})")]
    public static partial void FileWritingFailed(this ILogger logger,
                                                 FileUpdateRequest request,
                                                 int attempt,
                                                 Exception ex);

    [LoggerMessage(EventId = 3, Level = LogLevel.Trace, Message = "Request made fits the buffer allocated to it")]
    public static partial void RequestFitsBuffer(this ILogger logger);

    [LoggerMessage(EventId = 4,
                   Level = LogLevel.Information,
                   Message = "Request made exceeded the default buffer size and allocated a new buffer.")]
    public static partial void RequestExceededBuffer(this ILogger logger);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "Requested a write of a new item ({request})")]
    public static partial void RequestQueued(this ILogger logger, FileUpdateRequest request);
}
