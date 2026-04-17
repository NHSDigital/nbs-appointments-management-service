using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class RolesStore(ITypedDocumentCosmosStore<RolesDocument> cosmosStore) : IRolesStore
{
    private const string GlobalRolesDocumentId = "global_roles";
    
    public async Task<IEnumerable<Core.Users.Role>> GetRoles()
    {
        var rolesDocument = await cosmosStore.GetByIdAsync(GlobalRolesDocumentId);
        return rolesDocument.Roles.Select(r => new Core.Users.Role
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Permissions = r.Permissions
        });
    }
}
