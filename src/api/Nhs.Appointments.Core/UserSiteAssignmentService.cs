namespace Nhs.Appointments.Core;

public interface IUserSiteAssignmentService
{    
    Task<IEnumerable<string>> GetUserAssignedSites(string userId);
}

public class UserSiteAssignmentService(IUserSiteAssignmentStore store) : IUserSiteAssignmentService
{
    private readonly IUserSiteAssignmentStore _store = store;    

    public Task<IEnumerable<string>> GetUserAssignedSites(string userId) 
    {
        return _store.GetUserAssignedSites(userId);        
    }
}
