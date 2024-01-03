using Willow.Core.Settings.Models;

namespace Willow.Core.Settings.Abstractions;

internal interface IQueuedFileWriter
{
    void QueueRequest(FileUpdateRequest request);
    void Flush();
}