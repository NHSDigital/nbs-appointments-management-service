namespace Nhs.Appointments.Core;

public interface IUserStore
{
    Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId);
    Task<string> GetApiUserSigningKey(string clientId);
    Task<RoleAssignment[]> UpdateUserRoleAssignments(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments);
    Task SaveUserAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments);
    Task<IEnumerable<User>> GetUsersAsync(string site);
    Task<User> GetOrDefaultAsync(string userId);
    Task<OperationResult> RemoveUserAsync(string userId, string siteId);
}
