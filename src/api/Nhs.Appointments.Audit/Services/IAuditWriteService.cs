using Nhs.Appointments.Audit.Persistance;

namespace Nhs.Appointments.Audit.Services;

public interface IAuditWriteService
{
    public Task RecordFunction(string id, DateTime timestamp, string user, string functionName, string site);

    public Task RecordAuth(string id, DateTime timestamp, string user, AuditAuthActionType actionType);
}
