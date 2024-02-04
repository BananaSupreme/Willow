namespace Willow.Registration;

/// <summary>
/// Background worker that gets started when the assembly is loaded, and stopped when it is unloaded.
/// </summary>
/// <remarks>
/// Loaded via <see cref="IAssemblyRegistrar"/>
/// </remarks>
public interface IBackgroundWorker
{
    /// <summary>
    /// Starts the worker.
    /// </summary>
    /// <param name="cancellationToken">
    /// This token will be cancelled before stop is called, for long running processes it can be used as a hint to stop processing.
    /// </param>
    /// <remarks>
    /// The system checks for exception only in the start of the processing, implementors should make sure to fail quick
    /// if needed, otherwise exceptions are swallowed (Generally the worker should handle its exception and not trust on
    /// failing the system).
    /// </remarks>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the background service.
    /// </summary>
    /// <remarks>
    /// Exceptions here are completely ignored.
    /// </remarks>
    Task StopAsync();
}
