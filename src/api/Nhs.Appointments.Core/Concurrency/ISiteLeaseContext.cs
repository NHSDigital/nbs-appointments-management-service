namespace Nhs.Appointments.Core.Concurrency;

public interface ISiteLeaseContext : IDisposable
{
    string SiteKey { get; }
}
