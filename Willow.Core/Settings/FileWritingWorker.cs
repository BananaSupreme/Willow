using System.Text;
using System.Threading.Channels;

using Microsoft.Extensions.Hosting;

using Willow.Core.Settings.Models;

namespace Willow.Core.Settings;

internal sealed class FileWritingWorker : BackgroundService, IQueuedFileWriter
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
            ProcessUpdateAsync(request, CancellationToken.None).GetAwaiter().GetResult();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(async () => await AcceptFilesAsync(stoppingToken), stoppingToken);
    }

    private async Task AcceptFilesAsync(CancellationToken cancellationToken)
    {
        await foreach (var request in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            var attempt = 0;
            while (attempt < 10)
            {
                _log.ProcessingRequest(request);
                try
                {
                    await ProcessUpdateAsync(request, cancellationToken);
                    break;
                }
                catch (Exception ex)
                {
                    _log.FileWritingFailed(request, attempt, ex);
                }

                await Task.Delay((int)Math.Pow(100, attempt), cancellationToken);
                attempt++;
            }
        }
    }

    private async Task ProcessUpdateAsync(FileUpdateRequest request, CancellationToken cancellationToken)
    {
        request.File.Seek(0, SeekOrigin.Begin);
        if (request.Value.Length < _buffer.Length)
        {
            _log.RequestFitsBuffer();
            Encoding.UTF8.GetBytes(request.Value.AsSpan(), _buffer.AsSpan());
            var buffer = _buffer.AsMemory()[..request.Value.Length];
            await request.File.WriteAsync(buffer, cancellationToken);
            await request.File.FlushAsync(cancellationToken);
            _buffer.AsSpan().Clear();
            return;
        }

        _log.RequestExceededBuffer();
        var bytes = Encoding.UTF8.GetBytes(request.Value);
        await request.File.WriteAsync(bytes, cancellationToken);
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
