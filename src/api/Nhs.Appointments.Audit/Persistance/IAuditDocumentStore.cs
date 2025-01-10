namespace Nhs.Appointments.Audit.Persistance;

public interface IAuditDocumentStore 
{
    Task InsertAsync(AuditFunctionDocument auditFunctionDocument);
}
