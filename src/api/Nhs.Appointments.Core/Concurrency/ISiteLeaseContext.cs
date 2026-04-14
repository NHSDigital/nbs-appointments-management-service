namespace Nhs.Appointments.Core.Concurrency;

public interface ISiteLeaseContext : IDisposable
{
    public string SiteKey { get; }
}
