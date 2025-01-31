namespace Nhs.Appointments.Core;

public interface IUserService
{
    Task<User> GetUserAsync(string userId);
    Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId);
    Task<string> GetApiUserSigningKey(string clientId);
    Task<UpdateUserRoleAssignmentsResult> UpdateUserRoleAssignmentsAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments);
    Task<IEnumerable<User>> GetUsersAsync(string site);
    Task<OperationResult> RemoveUserAsync(string userId, string site);
}
