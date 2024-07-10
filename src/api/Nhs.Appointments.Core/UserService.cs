namespace Nhs.Appointments.Core;

public interface IUserService
{    
    Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId);
}

public class UserService(IUserStore store) : IUserService
{
    public Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId)
    {
        return store.GetUserRoleAssignments(userId);
    }
}
