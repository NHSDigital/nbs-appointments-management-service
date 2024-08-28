using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core;

public interface IUserService
{    
    Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId);
    Task<string> GetApiUserSigningKey(string clientId);
    Task UpdateUserRoleAssignmentsAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments);
    Task SaveUserAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments);
    Task<IEnumerable<User>> GetUsersAsync(string site);
}

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

    public async Task UpdateUserRoleAssignmentsAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments)
    {
        var oldRoles = await store.UpdateUserRoleAssignments(userId, scope, roleAssignments);

        var rolesRemoved = oldRoles.Where(old => !roleAssignments.Any(r => r.Role == old.Role));
        var rolesAdded = roleAssignments.Where(newRole => !oldRoles.Any(r => r.Role == newRole.Role));

        await bus.Send(new UserRolesChanged { User = userId, Added = rolesAdded.Select(r => r.Role).ToArray(), Removed = rolesRemoved.Select(r => r.Role).ToArray()});
    }

    public async Task SaveUserAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments)
    {
        await store.SaveUserAsync(userId, scope, roleAssignments);
    }
    
    public Task<IEnumerable<User>> GetUsersAsync(string site) 
    { 
        return store.GetUsersAsync(site);
    }
}
