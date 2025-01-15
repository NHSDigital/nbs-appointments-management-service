using Nhs.Appointments.Audit.Persistance;
using Nhs.Appointments.Persistance;

namespace Nhs.Appointments.Audit.Services;

public class AuditWriteService(ITypedDocumentCosmosStore<AuditFunctionDocument> auditFunctionStore, 
    ITypedDocumentCosmosStore<AuditAuthDocument> auditAuthStore) : IAuditWriteService
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
            Site = site
        };

        await auditFunctionStore.WriteAsync(doc);
    }
    
    public async Task RecordAuth(string id, DateTime timestamp, string user, AuditAuthActionType actionType)
    {
        if (actionType == AuditAuthActionType.Undefined)
        {
            throw new ArgumentException("AuditAuthActionType is undefined.", nameof(actionType));
        }
        
        var docType = auditAuthStore.GetDocumentType();
        var doc = new AuditAuthDocument
        {
            Id = id,
            DocumentType = docType,
            ActionType = actionType,
            Timestamp = timestamp,
            User = user
        };

        await auditAuthStore.WriteAsync(doc);
    }
}
