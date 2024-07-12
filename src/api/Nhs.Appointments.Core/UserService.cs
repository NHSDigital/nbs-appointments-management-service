namespace Nhs.Appointments.Core;

public interface IUserService
{    
    Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId);
    Task SaveUserAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments);
}

public class UserService(IUserStore store) : IUserService
{
    public Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId)
    {
        return store.GetUserRoleAssignments(userId);
    }
    
    public Task SaveUserAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments)
    {
        return store.SaveUserAsync(userId, scope, roleAssignments);
    }
}
