using System;
using System.Linq;
using System.Threading.Tasks;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Auth;

public class PermissionChecker(IUserService userService, IRolesService rolesService) : IPermissionChecker
{
    public async Task<bool> HasPermissionAsync(string userId, string requiredPermission)
    {
        var roles = await rolesService.GetRoles();
        var userAssignments = await userService.GetUserRoleAssignments(userId);
        var userRoles = userAssignments.Where(ra => ra.Scope == "global").Select(ra => ra.Role);
        var usersPermissions = roles.Where(r => userRoles.Contains(r.Id)).SelectMany(r => r.Permissions).Distinct();
        return usersPermissions.Contains(requiredPermission);
    }
}
