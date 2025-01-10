using Nhs.Appointments.Persistance;

namespace Nhs.Appointments.Audit.Persistance;

public class AuditCosmosDocumentStore(ITypedDocumentCosmosStore<AuditFunctionDocument> auditFunctionStore) : IAuditDocumentStore
{
    public async Task InsertAsync(AuditFunctionDocument auditFunctionDocument)
    {
        await auditFunctionStore.WriteAsync(auditFunctionDocument);
    }
}
