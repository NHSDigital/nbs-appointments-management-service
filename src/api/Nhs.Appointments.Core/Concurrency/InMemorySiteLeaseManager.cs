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
            if (_locks.ContainsKey(keyName) == false)
            {
                _locks.Add(keyName, new SemaphoreSlim(1,1));
            }
            mutex = _locks[keyName];
        }

        if (mutex.Wait(_options.Timeout) == false)
        {
            throw new AbandonedMutexException();
        }

        return new SiteLeaseContext(() => mutex.Release());
    }        
}
