namespace Nhs.Appointments.Core;

public interface IUserStore
{
    Task<User> GetUserAsync(string userId);
    Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId);
    Task<string> GetApiUserSigningKey(string clientId);
    Task UpdateUserRoleAssignments(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments);
    Task<IEnumerable<User>> GetUsersAsync(string site);
    Task<User> GetOrDefaultAsync(string userId);
    Task<OperationResult> RemoveUserAsync(string userId, string siteId);

    Task<OperationResult> RecordEulaAgreementAsync(string userId, DateOnly versionDate);
    Task UpdateUserRegionPermissionsAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments);
    Task SaveAdminUserAsync(User adminUser);
    Task RemoveAdminUserAsync(string userId);
    Task UpdateUserIcbPermissionsAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments);
}
