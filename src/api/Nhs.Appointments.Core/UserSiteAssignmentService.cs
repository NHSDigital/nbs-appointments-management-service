namespace Nhs.Appointments.Core;

public interface IUserSiteAssignmentService
{
    Task<string> GetSiteIdForUserByEmailAsync(string userEmail);
}

public class UserSiteAssignmentService : IUserSiteAssignmentService
{
    private readonly IUserSiteAssignmentStore _store;

    public UserSiteAssignmentService(IUserSiteAssignmentStore store)
    {
        _store = store;
    }

    public Task<string> GetSiteIdForUserByEmailAsync(string userEmail)
    {
        return _store.GetSiteIdForUserByEmailAsync(userEmail);
    }
}
