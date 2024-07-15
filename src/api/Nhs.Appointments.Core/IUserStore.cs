namespace Nhs.Appointments.Core;

public interface IUserStore
{
    Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId);
    Task<string> GetApiUserSigningKey(string clientId);
}
