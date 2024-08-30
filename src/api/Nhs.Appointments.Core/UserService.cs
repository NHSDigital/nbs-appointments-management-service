using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core;

public class UserService(IUserStore store, IMessageBus bus) : IUserService
{
    public Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId)
    {
        return store.GetUserRoleAssignments(userId);
    }

    public Task<string> GetApiUserSigningKey(string clientId)
    {
        return store.GetApiUserSigningKey(clientId);
    }

    public async Task UpdateUserRoleAssignmentsAsync(string userId, string site, string scope, IEnumerable<RoleAssignment> roleAssignments)
    {
        var oldRoles = await store.UpdateUserRoleAssignments(userId, scope, roleAssignments);

        var rolesRemoved = oldRoles.Where(old => !roleAssignments.Any(r => r.Role == old.Role));
        var rolesAdded = roleAssignments.Where(newRole => !oldRoles.Any(r => r.Role == newRole.Role));

        await bus.Send(new UserRolesChanged { User = userId, Site = site, Added = rolesAdded.Select(r => r.Role).ToArray(), Removed = rolesRemoved.Select(r => r.Role).ToArray()});
    }

    public Task<IEnumerable<User>> GetUsersAsync(string site) 
    { 
        return store.GetUsersAsync(site);
    }
}
