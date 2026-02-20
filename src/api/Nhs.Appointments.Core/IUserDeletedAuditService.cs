namespace Nhs.Appointments.Core;

public interface IUserDeletedAuditService
{
    Task RecordUserDeleted(string userId, string scope, string removedBy);
}
