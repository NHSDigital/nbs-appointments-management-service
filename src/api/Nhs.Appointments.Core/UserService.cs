namespace Nhs.Appointments.Core;

public interface IUserService
{    
    Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId);
    Task<string> GetApiUserSigningKey(string clientId);
    Task SaveUserAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments);
    Task<IEnumerable<User>> GetUsersAsync(string site);
}

public class UserService(IUserStore store) : IUserService
{
    public Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId)
    {
        return store.GetUserRoleAssignments(userId);
    }

    public Task<string> GetApiUserSigningKey(string clientId)
    {
        return store.GetApiUserSigningKey(clientId);
    }
    
    public Task SaveUserAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments)
    {
        return store.SaveUserAsync(userId, scope, roleAssignments);
    }
    
    public Task<IEnumerable<User>> GetUsersAsync(string site) 
    { 
        return store.GetUsersAsync(site);
    }
}
