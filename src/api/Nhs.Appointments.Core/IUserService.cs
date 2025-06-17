namespace Nhs.Appointments.Core;

public interface IUserService
{
    Task<User> GetUserAsync(string userId);
    Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId);
    Task<string> GetApiUserSigningKey(string clientId);
    Task<UpdateUserRoleAssignmentsResult> UpdateUserRoleAssignmentsAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments, bool sendNotifications = true);
    Task<IEnumerable<User>> GetUsersAsync(string site);
    Task<OperationResult> RemoveUserAsync(string userId, string site);
    Task SaveUserAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments);
    Task<UserIdentityStatus> GetUserIdentityStatusAsync(string siteId, string userId);
    Task UpdateRegionalUserRoleAssignmentsAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments);
}
