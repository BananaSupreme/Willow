using Microsoft.Extensions.Hosting;

using System.Text;
using System.Threading.Channels;

using Willow.Core.Settings.Models;

namespace Willow.Core.Settings;

internal class FileWritingWorker : BackgroundService, IQueuedFileWriter
{
    private readonly Channel<FileUpdateRequest> _channel = Channel.CreateUnbounded<FileUpdateRequest>();
    private readonly byte[] _buffer = new byte[4096];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(async () => await AcceptFilesAsync(stoppingToken), stoppingToken);
    }

    private async Task AcceptFilesAsync(CancellationToken cancellationToken)
    {
        await foreach (var request in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            await ProcessUpdateAsync(request, cancellationToken);
        }
    }

    private async Task ProcessUpdateAsync(FileUpdateRequest request, CancellationToken cancellationToken)
    {
        request.File.Seek(0, SeekOrigin.Begin);
        if (request.Value.Length < _buffer.Length)
        {
            Encoding.UTF8.GetBytes(request.Value.AsSpan(), _buffer.AsSpan());
            var buffer = _buffer.AsMemory()[..request.Value.Length];
            await request.File.WriteAsync(buffer, cancellationToken);
            await request.File.FlushAsync(cancellationToken);
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(request.Value);
        await request.File.WriteAsync(bytes, cancellationToken);
    }

    public void QueueRequest(FileUpdateRequest request)
    {
        _channel.Writer.TryWrite(request);
    }

    public void Flush()
    {
        while (_channel.Reader.TryRead(out var request))
        {
            ProcessUpdateAsync(request, CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}