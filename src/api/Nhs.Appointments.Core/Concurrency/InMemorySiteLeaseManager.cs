using Microsoft.Extensions.Options;

namespace Nhs.Appointments.Core.Concurrency;

internal class InMemorySiteLeaseManager : ISiteLeaseManager
{
    private readonly Dictionary<string, SemaphoreSlim> _locks;
    private readonly SiteLeaseManagerOptions _options;

    public InMemorySiteLeaseManager(IOptions<SiteLeaseManagerOptions> options)
    {
        _locks = new Dictionary<string, SemaphoreSlim>();
        _options = options.Value;
    }

    public ISiteLeaseContext Acquire(string site, DateOnly date)
    {
        SemaphoreSlim mutex;

        var keyName = LeaseKeys.SiteKeyFactory.Create(site, date);

        lock (_locks)
        {
            if (!_locks.ContainsKey(keyName))
            {
                _locks.Add(keyName, new SemaphoreSlim(1,1));
            }
            mutex = _locks[keyName];
        }

        if (!mutex.Wait(_options.Timeout))
        {
            throw new AbandonedMutexException($"Abandoned attempt to acquire lock for site key {keyName}");
        }

        return new SiteLeaseContext(keyName, () => mutex.Release());
    }
}
