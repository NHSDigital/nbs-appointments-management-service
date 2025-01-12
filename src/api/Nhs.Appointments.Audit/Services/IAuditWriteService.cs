using Nhs.Appointments.Audit.Persistance;

namespace Nhs.Appointments.Audit.Services;

public interface IAuditWriteService
{
    public Task RecordFunction(DateTime timestamp, string userId, string functionName, string siteId);
}
