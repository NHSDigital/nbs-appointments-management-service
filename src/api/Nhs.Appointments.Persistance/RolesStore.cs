using AutoMapper;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class RolesStore(ITypedDocumentCosmosStore<RolesDocument> cosmosStore, IMapper mapper) : IRolesStore
{
    private const string DocumentId = "global_roles";
    
    public async Task<IEnumerable<Core.Role>> GetRoles()
    {
        var rolesDocument = await cosmosStore.GetByIdAsync<RolesDocument>(DocumentId);
        return rolesDocument.Roles.Select(mapper.Map<Core.Role>);
    }
}