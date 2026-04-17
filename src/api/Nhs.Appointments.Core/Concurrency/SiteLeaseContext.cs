namespace Nhs.Appointments.Core.Concurrency;

public class SiteLeaseContext : ISiteLeaseContext
{
    private readonly Action _release;

    public SiteLeaseContext(string siteKey, Action release)
    {
        ArgumentException.ThrowIfNullOrEmpty(siteKey, nameof(siteKey));
        ArgumentNullException.ThrowIfNull(release, nameof(release));

        SiteKey = siteKey;
        _release = release;
    }

    public void Dispose()
    {
        _release();
    }

    public string SiteKey { get; private set; }
}
