namespace Nhs.Appointments.Core.Sites;

public class SiteCacheLock
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task WaitAsync() => await _semaphore.WaitAsync();

    public void Release() => _semaphore.Release();
}
