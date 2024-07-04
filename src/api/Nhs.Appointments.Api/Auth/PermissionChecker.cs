using System;
using System.Linq;
using System.Threading.Tasks;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Auth;

public class PermissionChecker(IUserService userService, IRolesService rolesService) : IPermissionChecker
{
    public async Task<bool> HasPermissionAsync(string userId, string requiredPermission, string siteId)
    {
        var roles = await rolesService.GetRoles();
        var userRoleAssignments = await userService.GetUserRoleAssignments(userId);
        Func<RoleAssignment, bool> filter = string.IsNullOrEmpty(siteId) ? ra => ra.Scope == "global" : ra => ra.Scope == "global" || ra.Scope == $"site:{siteId}";
        var userRoles = userRoleAssignments.Where(filter).Select(ra => ra.Role);
        var usersPermissions = roles.Where(r => userRoles.Contains(r.Id)).SelectMany(r => r.Permissions).Distinct();
        return usersPermissions.Contains(requiredPermission);
    }
}
