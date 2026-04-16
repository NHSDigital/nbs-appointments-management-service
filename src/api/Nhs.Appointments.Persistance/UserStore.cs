using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Users;
using Nhs.Appointments.Persistance.Models;
using RoleAssignment = Nhs.Appointments.Core.Users.RoleAssignment;
using User = Nhs.Appointments.Core.Users.User;

namespace Nhs.Appointments.Persistance;

public class UserStore(ITypedDocumentCosmosStore<UserDocument> cosmosStore) : IUserStore
{
    private const string IcbUserRole = "system:icb-user";
    private const string RegionalUserRole = "system:regional-user";

    public async Task<string> GetApiUserSigningKey(string clientId)
    {
        var documentId = $"api@{clientId}";
        var userDocument = await cosmosStore.GetByIdAsync(documentId);
        return userDocument.ApiSigningKey;
    }

    public async Task<User> GetUserAsync(string userId)
    {
        var userDocument = await cosmosStore.GetByIdOrDefaultAsync(userId.ToLower());
        return userDocument is null ? null : MapToUser(userDocument);
    }

    public async Task<IEnumerable<RoleAssignment>> GetUserRoleAssignments(string userId)
    {
        var userDocument = await cosmosStore.GetByIdOrDefaultAsync(userId.ToLower());
        return userDocument is not null ? userDocument.RoleAssignments.Select(MapToRoleAssignment) : Array.Empty<RoleAssignment>();
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
            await cosmosStore.PatchDocument(documentType, userId.ToLower(), userDocumentPatch);
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
        await cosmosStore.PatchDocument(cosmosStore.GetDocumentType(), userId.ToLower(), userDocumentPatch);
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

        var updatedRoleAssignments = GetUpdatedPermissions(originalDocument.RoleAssignments, scope, roleAssignments, RegionalUserRole, IcbUserRole);

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
        await cosmosStore.PatchDocument(cosmosStore.GetDocumentType(), userId.ToLower(), updateEulaPatch);
        return new OperationResult(true);
    }

    public async Task<IEnumerable<User>> GetUsersAsync(string site)
    {
        var userDocuments = await cosmosStore.RunQueryAsync(usr => usr.DocumentType == "user" && usr.RoleAssignments.Any(ra => ra.Scope == $"site:{site}"));
        return userDocuments?.Select(MapToUser) ?? [];
    }
    
    private async Task InsertAsync(User user)
    {
        var document = MapToUserDocument(user);
        await cosmosStore.WriteAsync(document);
    }

    public async Task<User> GetOrDefaultAsync(string userId)
    {
        var userDocument = await cosmosStore.GetByIdOrDefaultAsync(userId.ToLower());
        return userDocument is null ? null : MapToUser(userDocument);
    }

    public async Task SaveAdminUserAsync(User adminUser)
        => await InsertAsync(adminUser);

    public async Task RemoveAdminUserAsync(string userId) =>
        await cosmosStore.DeleteDocument(userId.ToLower(), cosmosStore.GetDocumentType());

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

        var updatedRoleAssignments = GetUpdatedPermissions(originalDocument.RoleAssignments, scope, roleAssignments, IcbUserRole, RegionalUserRole);

        var icbRolePatch = PatchOperation.Set("/roleAssignments", updatedRoleAssignments);
        await cosmosStore.PatchDocument(cosmosStore.GetDocumentType(), userId, icbRolePatch);
    }

    internal IEnumerable<RoleAssignment> GetUpdatedPermissions(
        IEnumerable<RoleAssignment> existingRoleAssignments,
        string scope,
        IEnumerable<RoleAssignment> newRoleAssignments,
        string primaryRole,
        string conflictingRole)
    {
        var updatedRoleAssignments = existingRoleAssignments.AsEnumerable();

        var hasPrimaryScopedPermission = updatedRoleAssignments.Any(ra => ra.Scope == scope);
        var hasOtherPrimaryScopedPermission = updatedRoleAssignments.Any(ra => ra.Role == primaryRole && ra.Scope != scope);
        var hasConflictingRole = updatedRoleAssignments.Any(ra => ra.Role == conflictingRole);

        // User already has this primary permission - treat as removal
        if (hasPrimaryScopedPermission)
        {
            updatedRoleAssignments = updatedRoleAssignments.Where(ra => ra.Scope != scope);
        }
        // Update the user's primary permission - only one allowed at a time
        else if (hasOtherPrimaryScopedPermission)
        {
            updatedRoleAssignments = updatedRoleAssignments
                .Where(ra => ra.Role != primaryRole)
                .Concat(newRoleAssignments);
        }
        // No existing primary permission - add it
        else
        {
            updatedRoleAssignments = updatedRoleAssignments.Concat(newRoleAssignments);
        }

        // If user has conflicting role, remove it unless we're removing the primary role in this update
        if (hasConflictingRole && !hasPrimaryScopedPermission)
        {
            updatedRoleAssignments = updatedRoleAssignments.Where(ra => ra.Role != conflictingRole);
        }

        return updatedRoleAssignments;
    }

    public async Task<IEnumerable<User>> GetUsersWithPermissionScope(string scope)
    {
        var docType = cosmosStore.GetDocumentType();

        var query = @"
                    SELECT * FROM c
                    WHERE c.docType = @docType
                    AND EXISTS (
                        SELECT 1 FROM a
                        IN c.roleAssignments
                        WHERE CONTAINS(a['scope'], @scope)
                    )";

            var queryDefinition = new QueryDefinition(query)
                .WithParameter("@docType", docType)
                .WithParameter("@scope", scope);

            var userDocuments = await cosmosStore.RunSqlQueryAsync(queryDefinition);
            return userDocuments?.Select(MapToUser) ?? [];
        }
    }

    private static UserDocument MapToUserDocument(User user)
    {
        var userDocument = new UserDocument
        {
            Id = user.Id.ToLower(),
            RoleAssignments = [.. user.RoleAssignments.Select(ra => new Models.RoleAssignment
            {
                Role = ra.Role,
                Scope = ra.Scope
            })],
        };

        if (user.LatestAcceptedEulaVersion.HasValue)
        {
            userDocument.LatestAcceptedEulaVersion = user.LatestAcceptedEulaVersion.Value;
        }
        
        return userDocument;
    }

    private static User MapToUser(UserDocument userDocument)
    {
        var user = new User { Id = userDocument?.Id };

        if (userDocument.RoleAssignments?.Length > 0)
        {
            user.RoleAssignments = [.. userDocument.RoleAssignments.Select(ra => new RoleAssignment
            {
                Role = ra?.Role,
                Scope = ra?.Scope
            })];
        }

        if (userDocument?.LatestAcceptedEulaVersion != default)
        {
            user.LatestAcceptedEulaVersion = userDocument.LatestAcceptedEulaVersion;
        }

        return user;
    }

    private static RoleAssignment MapToRoleAssignment(Models.RoleAssignment roleAssignment)
    {
        return new RoleAssignment
        {
            Role = roleAssignment.Role,
            Scope = roleAssignment.Scope
        };
    }
}
