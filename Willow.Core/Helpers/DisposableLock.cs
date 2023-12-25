namespace Willow.Core.Helpers;

public readonly struct DisposableLock : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public DisposableLock() { }

    public SemaphoreReleaser Lock()
    {
        _semaphore.Wait();
        return new(_semaphore);
    }
    
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