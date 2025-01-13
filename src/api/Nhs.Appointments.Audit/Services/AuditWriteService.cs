using Nhs.Appointments.Audit.Persistance;
using Nhs.Appointments.Persistance;

namespace Nhs.Appointments.Audit.Services;

public class AuditWriteService(ITypedDocumentCosmosStore<AuditFunctionDocument> auditFunctionStore) : IAuditWriteService
{
    public async Task RecordFunction(string id, DateTime timestamp, string user, string functionName, string site)
    {
        var docType = auditFunctionStore.GetDocumentType();
        var doc = new AuditFunctionDocument
        {
            Id = id,
            DocumentType = docType,
            Timestamp = timestamp,
            User = user,
            FunctionName = functionName,
            Site = site,
        };

        await auditFunctionStore.WriteAsync(doc);
    }
}
