namespace Nhs.Appointments.Core;

public interface IUserStore
{
    Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId);
    Task SaveUserAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments);
}
