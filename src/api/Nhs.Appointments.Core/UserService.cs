using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core;

public class UserService(IUserStore userStore, IRolesStore rolesStore, IMessageBus bus) : IUserService
{
    public Task<User> GetUserAsync(string userId)
    {
        return userStore.GetUserAsync(userId);
    }

    public Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId)
    {
        return userStore.GetUserRoleAssignments(userId);
    }

    public Task<string> GetApiUserSigningKey(string clientId)
    {
        return userStore.GetApiUserSigningKey(clientId);
    }

    public async Task<UpdateUserRoleAssignmentsResult> UpdateUserRoleAssignmentsAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments)
    {
        var allRoles = await rolesStore.GetRoles();
        var lowerUserId = userId.ToLower();
        var invalidRoles = roleAssignments.Where(ra => !allRoles.Any(r => r.Id == ra.Role));
        if (invalidRoles.Any())
        {
            return new UpdateUserRoleAssignmentsResult(false, "", invalidRoles.Select(ra => ra.Role));
        }

        var oldRoles = await userStore.UpdateUserRoleAssignments(lowerUserId, scope, roleAssignments);
        IEnumerable<RoleAssignment> rolesRemoved = [];
        IEnumerable<RoleAssignment> rolesAdded;

        // New user
        if (oldRoles.Length == 0)
        {
            rolesAdded = roleAssignments;
        }
        else
        {
            rolesRemoved = oldRoles.Where(old => !roleAssignments.Any(r => r.Role == old.Role));
            rolesAdded = roleAssignments.Where(newRole => !oldRoles.Any(r => r.Role == newRole.Role));
        }

        var site = Scope.GetValue("site", scope);

        if (lowerUserId.EndsWith("@nhs.net"))
        {
            //NHS user
            await bus.Send(new UserRolesChanged { UserId = lowerUserId, SiteId = site, AddedRoleIds = rolesAdded.Select(r => r.Role).ToArray(), RemovedRoleIds = rolesRemoved.Select(r => r.Role).ToArray()});
        }
        else
        {
            //OKTA user
            await bus.Send(new OktaUserRolesChanged { UserId = lowerUserId, SiteId = site, AddedRoleIds = rolesAdded.Select(r => r.Role).ToArray(), RemovedRoleIds = rolesRemoved.Select(r => r.Role).ToArray() });
        }

        return new UpdateUserRoleAssignmentsResult(true, string.Empty, []);
    }

    public Task<IEnumerable<User>> GetUsersAsync(string site) 
    { 
        return userStore.GetUsersAsync(site);
    }

    public Task<OperationResult> RemoveUserAsync(string userId, string site)
    {
        return userStore.RemoveUserAsync(userId, site);
    }

    public Task SaveUserAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments)
        => userStore.SaveUserAsync(userId, scope, roleAssignments);
}

public record UpdateUserRoleAssignmentsResult(bool success, string errorUser, IEnumerable<string> errorRoles)
{
    public bool Success => success;
    public string ErrorUser => errorUser ?? string.Empty;
    public string[] ErrorRoles => errorRoles.ToArray() ?? [];
}
