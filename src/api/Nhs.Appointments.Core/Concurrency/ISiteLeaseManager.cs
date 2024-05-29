namespace Nhs.Appointments.Core.Concurrency;

public interface ISiteLeaseManager
{
    ISiteLeaseContext Acquire(string site);
}
