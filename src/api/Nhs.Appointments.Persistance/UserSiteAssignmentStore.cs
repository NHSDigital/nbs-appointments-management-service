using AutoMapper;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class UserSiteAssignmentStore(ITypedDocumentCosmosStore<UserSiteAssignmentDocument> cosmosStore, IMapper mapper) : IUserSiteAssignmentStore
{
    private const string DocumentId = "assignments";
    
    public async Task<IEnumerable<UserAssignment>> GetUserAssignedSites(string userId)
    {
        var assignmentsDocument = await cosmosStore.GetByIdAsync<UserSiteAssignmentDocument>(DocumentId);
        return assignmentsDocument.Assignments.Where(x => x.Email == userId).Select(mapper.Map<UserAssignment>);
    }
}
