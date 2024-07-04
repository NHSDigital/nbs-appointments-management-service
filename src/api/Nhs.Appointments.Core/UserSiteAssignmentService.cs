namespace Nhs.Appointments.Core;

public interface IUserSiteAssignmentService
{    
    Task<IEnumerable<UserAssignment>> GetUserAssignedSites(string userId);
}

public class UserSiteAssignmentService(IUserSiteAssignmentStore store) : IUserSiteAssignmentService
{
    public Task<IEnumerable<UserAssignment>> GetUserAssignedSites(string userId)
    {
        return store.GetUserAssignedSites(userId);
    }
}
