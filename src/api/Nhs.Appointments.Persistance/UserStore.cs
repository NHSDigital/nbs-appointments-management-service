using AutoMapper;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using RoleAssignment = Nhs.Appointments.Core.RoleAssignment;
using User = Nhs.Appointments.Core.User;

namespace Nhs.Appointments.Persistance;

public class UserStore(ITypedDocumentCosmosStore<UserDocument> cosmosStore, IMapper mapper) : IUserStore
{
    private const string IcbUserRole = "system:icb-user";
    private const string RegionalUserRole = "system:regional-user";

    public async Task<string> GetApiUserSigningKey(string clientId)
    {
        var documentId = $"api@{clientId}";
        var userDocument = await cosmosStore.GetByIdAsync<UserDocument>(documentId);
        return userDocument.ApiSigningKey;
    }

    public async Task<User> GetUserAsync(string userId)
    {
        return await cosmosStore.GetByIdOrDefaultAsync<User>(userId);
    }

    public async Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId)
    {
        var userDocument = await cosmosStore.GetByIdOrDefaultAsync<UserDocument>(userId);
        return userDocument is not null ? userDocument.RoleAssignments.Select(mapper.Map<RoleAssignment>) : Array.Empty<RoleAssignment>();
    }

    /// <summary>
    /// Updates the roles assigned to the user by replacing them with the roles provided to this function.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="scope"></param>
    /// <param name="roleAssignments"></param>
    /// <returns>The original role assignments for the scope prior to completion of the update operation</returns>
    /// <exception cref="Exception"></exception>
    public async Task UpdateUserRoleAssignments(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments)
    {
        var originalDocument = await GetOrDefaultAsync(userId);
        if (originalDocument == null)
        {
            var user = new User
            {
                Id = userId,
                RoleAssignments = roleAssignments.ToArray()
            };
            await InsertAsync(user);
        }
        else
        {
            var documentType = cosmosStore.GetDocumentType();
            var originalRoleAssignments = originalDocument.RoleAssignments;
            var newRoleAssignments = originalRoleAssignments
                .Where(ra => ra.Scope != scope)
                .Concat(roleAssignments);
            var userDocumentPatch = PatchOperation.Add("/roleAssignments", newRoleAssignments);
            await cosmosStore.PatchDocument(documentType, userId, userDocumentPatch);
        }
    }

    public async Task<OperationResult> RemoveUserAsync(string userId, string siteId)
    {
        var user = await GetOrDefaultAsync(userId);
        if (user is null)
        {
            return new OperationResult(false, "User not found");
        }

        if (user.RoleAssignments.All(role => role.Scope != $"site:{siteId}"))
        {
            return new OperationResult(false, $"User has no roles at site {siteId}");
        }

        var roleAssignmentsWithSiteRemoved = user.RoleAssignments
            .Where(ra => ra.Scope != $"site:{siteId}")
            .ToList();

        if (roleAssignmentsWithSiteRemoved.Count == 0)
        {
            await cosmosStore.DeleteDocument(userId, cosmosStore.GetDocumentType());
            return new OperationResult(true);
        }

        var userDocumentPatch = PatchOperation.Add("/roleAssignments", roleAssignmentsWithSiteRemoved);
        await cosmosStore.PatchDocument(cosmosStore.GetDocumentType(), userId, userDocumentPatch);
        return new OperationResult(true);
    }

    public async Task UpdateUserRegionPermissionsAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments)
    {
        var originalDocument = await GetOrDefaultAsync(userId);
        if (originalDocument is null)
        {
            var user = new User
            {
                Id = userId,
                RoleAssignments = roleAssignments.ToArray()
            };
            await InsertAsync(user);
            return;
        }

        var existingRoleAssignments = originalDocument.RoleAssignments;
        var updatedRoleAssignments = existingRoleAssignments.AsEnumerable();

        var hasRegionScopedPermission = updatedRoleAssignments.Any(ra => ra.Scope == scope);
        var hasOtherRegionScopedPermission = updatedRoleAssignments.Any(ra => ra.Role == RegionalUserRole && ra.Scope != scope);
        var hasIcbScopedPermission = updatedRoleAssignments.Any(ra => ra.Role == IcbUserRole);

        // User already has this regional permission - therefore this is treated as a removal
        if (hasRegionScopedPermission)
        {
            updatedRoleAssignments = updatedRoleAssignments.Where(ra => ra.Scope != scope);
        }
        // Update the user's regional permission - they're only allowed one at a time
        else if (hasOtherRegionScopedPermission)
        {
            updatedRoleAssignments = updatedRoleAssignments
                .Where(ra => ra.Role != RegionalUserRole)
                .Concat(roleAssignments);
        }
        // No existing regional permission - add it
        else
        {
            updatedRoleAssignments = updatedRoleAssignments.Concat(roleAssignments);
        }

        // If a user has an existing ICB role, remove it unless we're removing the Regional role in this update
        if (hasIcbScopedPermission && !hasRegionScopedPermission)
        {
            updatedRoleAssignments = updatedRoleAssignments.Where(ra => ra.Role != IcbUserRole);
        }

        var regionalRolePatch = PatchOperation.Set("/roleAssignments", updatedRoleAssignments);
        await cosmosStore.PatchDocument(cosmosStore.GetDocumentType(), userId, regionalRolePatch);
    }

    public async Task<OperationResult> RecordEulaAgreementAsync(string userId, DateOnly versionDate)
    {
        var user = await GetOrDefaultAsync(userId);
        if (user is null)
        {
            return new OperationResult(false, "User not found");
        }

        var updateEulaPatch = PatchOperation.Set("/latestAcceptedEulaVersion", $"{versionDate:yyyy-MM-dd}");
        await cosmosStore.PatchDocument(cosmosStore.GetDocumentType(), userId, updateEulaPatch);
        return new OperationResult(true);
    }

    public Task<IEnumerable<User>> GetUsersAsync(string site)
    {
        return cosmosStore.RunQueryAsync<User>(usr => usr.DocumentType == "user" && usr.RoleAssignments.Any(ra => ra.Scope == $"site:{site}"));
    }
    
    private Task InsertAsync(User user)
    {
        var document = cosmosStore.ConvertToDocument(user);
        return cosmosStore.WriteAsync(document);
    }

    public async Task<User> GetOrDefaultAsync(string userId)
    {
        return await cosmosStore.GetByIdOrDefaultAsync<User>(userId);
    }

    public async Task SaveAdminUserAsync(User adminUser)
        => await InsertAsync(adminUser);

    public async Task RemoveAdminUserAsync(string userId) =>
        await cosmosStore.DeleteDocument(userId, cosmosStore.GetDocumentType());

    public async Task UpdateUserIcbPermissionsAsync(string userId, string scope, IEnumerable<RoleAssignment> roleAssignments)
    {
        var originalDocument = await GetOrDefaultAsync(userId);
        if (originalDocument is null)
        {
            var user = new User
            {
                Id = userId,
                RoleAssignments = roleAssignments.ToArray()
            };
            await InsertAsync(user);
            return;
        }

        var existingRoleAssignments = originalDocument.RoleAssignments;
        var updatedRoleAssignments = existingRoleAssignments.AsEnumerable();

        var hasIcbScopedPermission = updatedRoleAssignments.Any(ra => ra.Scope == scope);
        var hasOtherIcbScopedPermission = updatedRoleAssignments.Any(ra => ra.Role == IcbUserRole && ra.Scope != scope);
        var hasRegionalPermission = updatedRoleAssignments.Any(ra => ra.Role == RegionalUserRole);

        // User already has this ICB permission - therefore this is treated as a removal
        if (hasIcbScopedPermission)
        {
            updatedRoleAssignments = updatedRoleAssignments.Where(ra => ra.Scope != scope);
        }
        // Update the user's ICB permission - they're only allowed one at a time
        else if (hasOtherIcbScopedPermission)
        {
            updatedRoleAssignments = updatedRoleAssignments
                .Where(ra => ra.Role != IcbUserRole)
                .Concat(roleAssignments);
        }
        // No existing ICB permission - add it
        else
        {
            updatedRoleAssignments = updatedRoleAssignments.Concat(roleAssignments);
        }

        // If a user has a regional role, remove it unless we're removing the ICB role in this update
        if (hasRegionalPermission && !hasIcbScopedPermission)
        {
            updatedRoleAssignments = updatedRoleAssignments.Where(ra => ra.Role != RegionalUserRole);
        }

        var icbRolePatch = PatchOperation.Set("/roleAssignments", updatedRoleAssignments);
        await cosmosStore.PatchDocument(cosmosStore.GetDocumentType(), userId, icbRolePatch);
    }
}
