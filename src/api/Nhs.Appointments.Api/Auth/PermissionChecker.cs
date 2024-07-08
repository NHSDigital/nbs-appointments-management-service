using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Auth;

public class PermissionChecker(IUserService userService, IRolesService rolesService) : IPermissionChecker
{
    public async Task<bool> HasPermissionAsync(string userId, string siteId, string requiredPermission)
    {
        var usersPermissions = await GetPermissionsAsync(userId, siteId);
        return usersPermissions.Contains(requiredPermission);
    }

    public async Task<IEnumerable<string>> GetPermissionsAsync(string userId, string siteId)
    {
        var rolesOp = await TryPattern.TryAsync(() => rolesService.GetRoles());
        if (rolesOp.Completed == false)
            return Enumerable.Empty<string>();
        var roles = rolesOp.Result;

        var userRoleAssignmentsOp = await TryPattern.TryAsync(() => userService.GetUserRoleAssignments(userId));
        if (userRoleAssignmentsOp.Completed == false)
            return Enumerable.Empty<string>();
        var userRoleAssignments = userRoleAssignmentsOp.Result;

        Func<RoleAssignment, bool> filter = string.IsNullOrEmpty(siteId) ? ra => ra.Scope == "global" : ra => ra.Scope == "global" || ra.Scope == $"site:{siteId}";
        var userRoles = userRoleAssignments.Where(filter).Select(ra => ra.Role);
        return roles.Where(r => userRoles.Contains(r.Id)).SelectMany(r => r.Permissions).Distinct();
    }
}
