using Nhs.Appointments.Audit.Persistance;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance;

namespace Nhs.Appointments.Audit.Services;

public class AuditWriteService(
    ITypedDocumentCosmosStore<AuditFunctionDocument> auditFunctionStore, 
    ITypedDocumentCosmosStore<AuditAuthDocument> auditAuthStore,
    ITypedDocumentCosmosStore<AuditNotificationDocument> auditNotificationStore,
    ITypedDocumentCosmosStore<AuditUserRemovedDocument> auditUserRemovedStore
) : IAuditWriteService, IUserDeletedAuditService
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

    public async Task RecordNotification(
        string id, 
        DateTime timestamp, 
        string user, 
        string destinationId,
        string notificationName,
        string template, 
        string notificationType, 
        string reference
    )
    {
        var docType = auditNotificationStore.GetDocumentType();
        var doc = new AuditNotificationDocument()
        {
            Id = id,
            DocumentType = docType,
            DestinationId = destinationId,
            NotificationName = notificationName,
            Template = template,
            NotificationType = notificationType,
            Reference = reference,
            Timestamp = timestamp,
            User = user
        };

        await auditNotificationStore.WriteAsync(doc);
    }

    public async Task RecordUserDeleted(string userId, string scope, string removedBy)
    {
        var docType = auditUserRemovedStore.GetDocumentType();
        var doc = new AuditUserRemovedDocument()
        {
            Id = Guid.NewGuid().ToString(),
            Scope = scope,
            User = userId,
            RemovedBy = removedBy,
            DocumentType = docType,
            Timestamp = DateTime.Now
        };

        await auditUserRemovedStore.WriteAsync(doc);
    }
}
