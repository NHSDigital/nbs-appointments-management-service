namespace Nhs.Appointments.Core;

public interface IUserStore
{
    Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId);
}
