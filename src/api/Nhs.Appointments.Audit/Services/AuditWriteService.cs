using Nhs.Appointments.Audit.Persistance;

namespace Nhs.Appointments.Audit.Services;

public class AuditWriteService(IAuditDocumentStore auditDocumentStore) : IAuditWriteService
{
    public async Task RecordFunction(DateTime timestamp, string userId, string functionName, string siteId)
    {
        await auditDocumentStore.InsertAsync(new AuditFunctionDocument()
        {
            Timestamp = timestamp,
            UserId = userId, 
            ActionType = functionName, 
            SiteId = siteId
        });
    }
}
