namespace Willow.Helpers.Locking;

/// <summary>
/// A wrapper around <see cref="SemaphoreSlim"/> that allows only one item to enter it at a time
/// using the <see langword="using"/> statement
/// </summary>
/// <remarks>
/// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
/// IT WITHOUT NOTICE!
/// </remarks>
public readonly struct DisposableLock : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Tests whether the lock can be entered, this is actually not thread-safe.
    /// </summary>
    public bool CanEnter => _semaphore.CurrentCount > 0;

    public DisposableLock() { }

    /// <summary>
    /// Acquires a lock.
    /// </summary>
    /// <returns>A disposable releaser for the semaphore.</returns>
    public SemaphoreReleaser Lock()
    {
        _semaphore.Wait();
        return new(_semaphore);
    }
    
    /// <summary>
    /// Acquires a lock asynchronously.
    /// </summary>
    /// <returns>returns a task containing a disposable releaser for the semaphore.</returns>
    public async Task<SemaphoreReleaser> LockAsync()
    {
        await _semaphore.WaitAsync();
        return new(_semaphore);
    }
    
    public void Dispose()
    {
        _semaphore.Dispose();
    }

    public readonly struct SemaphoreReleaser : IDisposable
    {
        private readonly SemaphoreSlim? _semaphore;

        public SemaphoreReleaser(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            _semaphore?.Release();
        }
    }
}