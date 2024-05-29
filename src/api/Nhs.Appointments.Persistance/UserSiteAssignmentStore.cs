using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class UserSiteAssignmentStore : IUserSiteAssignmentStore
{
    private const string DocumentId = "assignments";
    private readonly ITypedDocumentCosmosStore<UserSiteAssignmentDocument> _cosmosStore;

    public UserSiteAssignmentStore(ITypedDocumentCosmosStore<UserSiteAssignmentDocument> cosmosStore)
    {
        _cosmosStore = cosmosStore;
    }

    public async Task<string> GetSiteIdForUserByEmailAsync(string userEmail)
    {
        var assignmentsDocument = await _cosmosStore.GetByIdAsync<UserSiteAssignmentDocument>(DocumentId);
        return assignmentsDocument.Assignments.SingleOrDefault(x => x.Email == userEmail)?.Site;
    }
}
