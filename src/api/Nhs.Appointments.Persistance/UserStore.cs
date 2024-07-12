using AutoMapper;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using RoleAssignment = Nhs.Appointments.Core.RoleAssignment;
using User = Nhs.Appointments.Core.User;

namespace Nhs.Appointments.Persistance;

public class UserStore(ITypedDocumentCosmosStore<UserDocument> cosmosStore, IMapper mapper) : IUserStore
{
    public async Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId)
    {
        var userDocument = await cosmosStore.GetByIdAsync<UserDocument>(userId);
        return userDocument.RoleAssignments.Select(mapper.Map<RoleAssignment>);
    }
    
    public async Task SaveUserAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments)
    {
        var user = new User
        {
            Id = userId,
            RoleAssignments = roleAssignments.ToArray()
        };
        await InsertAsync(user);
    }
    
    private Task InsertAsync(User user)
    {
        var document = cosmosStore.ConvertToDocument(user);
        return cosmosStore.WriteAsync(document);
    }
}
