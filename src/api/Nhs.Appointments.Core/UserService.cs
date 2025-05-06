using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;
using Nhs.Appointments.Core.Okta;

namespace Nhs.Appointments.Core;

public class UserService(
    IUserStore userStore,
    IRolesStore rolesStore,
    IMessageBus bus,
    IOktaUserDirectory oktaStore,
    IEmailWhitelistStore whiteListStore)
    : IUserService
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
        var invalidRolesForScope = roleAssignments.Where(x => !x.Scope.Equals(scope));
        if (invalidRolesForScope.Any()) 
        {
            throw new InvalidOperationException($"Invalid Role assignments based on the passed scope {scope}. Scopes not expected - {string.Join(", ", invalidRolesForScope.Select(x => x.Scope).Distinct())}");
        }

        var allRoles = await rolesStore.GetRoles();
        var invalidRoles = roleAssignments.Where(ra => !allRoles.Any(r => r.Id == ra.Role));
        if (invalidRoles.Any())
        {
            return new UpdateUserRoleAssignmentsResult(false, "", invalidRoles.Select(ra => ra.Role));
        }

        var lowerUserId = userId.ToLower();

        var user = await userStore.GetUserAsync(lowerUserId);

        await userStore.UpdateUserRoleAssignments(lowerUserId, scope, roleAssignments);

        if (!scope.Equals("*"))
        {
            await NotifyRoleAssignmentChanged(lowerUserId, Scope.GetValue("site", scope), user?.RoleAssignments.Where(x => x.Scope.Equals(scope)) ?? [], roleAssignments);
        }

        return new UpdateUserRoleAssignmentsResult(true, string.Empty, []);
    }

    private async Task NotifyRoleAssignmentChanged(string userId, string site, IEnumerable<RoleAssignment> oldAssignments, IEnumerable<RoleAssignment> newAssignments) 
    {
        IEnumerable<RoleAssignment> rolesRemoved = [];
        IEnumerable<RoleAssignment> rolesAdded = [];

        // New user
        if (oldAssignments.Count() == 0)
        {
            rolesAdded = newAssignments;
        }
        else
        {
            rolesRemoved = oldAssignments.Where(old => !newAssignments.Any(r => r.Role == old.Role));
            rolesAdded = newAssignments.Where(newRole => !oldAssignments.Any(r => r.Role == newRole.Role));
        }

        if (userId.EndsWith("@nhs.net"))
        {
            //NHS user
            await bus.Send(new UserRolesChanged { UserId = userId, SiteId = site, AddedRoleIds = rolesAdded.Select(r => r.Role).ToArray(), RemovedRoleIds = rolesRemoved.Select(r => r.Role).ToArray() });
        }
        else
        {
            //OKTA user
            await bus.Send(new OktaUserRolesChanged { UserId = userId, SiteId = site, AddedRoleIds = rolesAdded.Select(r => r.Role).ToArray(), RemovedRoleIds = rolesRemoved.Select(r => r.Role).ToArray() });
        }
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
        => userStore.UpdateUserRoleAssignments(userId, scope, roleAssignments);

    public async Task<UserIdentityStatus> GetUserIdentityStatusAsync(string siteId, string userId)
    {
        var whitelistedEmails = await whiteListStore.GetWhitelistedEmails();
        var identityProvider = userId.ToLower().EndsWith("@nhs.net") ? IdentityProvider.NhsMail : IdentityProvider.Okta;

        return new UserIdentityStatus
        {
            IdentityProvider = identityProvider,
            ExtantInMya = await CheckIfUserExistsAtSite(siteId, userId),
            ExtantInIdentityProvider = await CheckIfUserExistsInIdentityServer(userId, identityProvider),
            MeetsWhitelistRequirements = whitelistedEmails.Any(userId.ToLower().EndsWith)
        };
    }

    private async Task<bool> CheckIfUserExistsAtSite(string siteId, string userId)
    {
        var userProfile = await userStore.GetOrDefaultAsync(userId);

        return userProfile is not null &&
               userProfile.RoleAssignments.Any(roleAssignment => roleAssignment.Scope == $"site:{siteId}");
    }

    private async Task<bool> CheckIfUserExistsInIdentityServer(string userId, IdentityProvider identityProvider)
    {
        switch (identityProvider)
        {
            case IdentityProvider.NhsMail:
                // We currently assume all @nhs.net email addresses are valid
                return true;
            case IdentityProvider.Okta:
                {
                    var userExistsInOkta = await oktaStore.GetUserAsync(userId);
                    return userExistsInOkta != null;
                }
            case IdentityProvider.Unknown:
            default:
                return false;
        }
    }
}

public record UpdateUserRoleAssignmentsResult(bool success, string errorUser, IEnumerable<string> errorRoles)
{
    public bool Success => success;
    public string ErrorUser => errorUser ?? string.Empty;
    public string[] ErrorRoles => errorRoles.ToArray() ?? [];
}
