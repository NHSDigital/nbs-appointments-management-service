using System;
using System.Linq;
using System.Threading.Tasks;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Auth;

public class PermissionChecker(IUserSiteAssignmentService userSiteAssignmentService, IRolesService rolesService) : IPermissionChecker
{
    public async Task<bool> HasPermissionAsync(string userId, string requiredPermission)
    {
        var roles = await rolesService.GetRoles();
        var userAssignments = await userSiteAssignmentService.GetUserAssignedSites(userId);
        var userRoles = userAssignments.SingleOrDefault(ua => ua.Site == "__global__")?.Roles ?? Array.Empty<string>();
        var usersPermissions = roles.Where(r => userRoles.Contains(r.Id)).SelectMany(r => r.Permissions).Distinct();
        return usersPermissions.Contains(requiredPermission);
    }
}
