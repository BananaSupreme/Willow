using Willow.Core.Settings.Models;

namespace Willow.Core.Settings.Abstractions;

/// <summary>
/// Queues the writing of files in the system to sync the settings in the background.
/// </summary>
internal interface IQueuedFileWriter
{
    /// <summary>
    /// Adds a request to queue the file to the background thread.
    /// </summary>
    /// <param name="request">The file update request.</param>
    void QueueRequest(FileUpdateRequest request);

    /// <summary>
    /// Flushes the queue, intended for testing purposes.
    /// </summary>
    void Flush();
}