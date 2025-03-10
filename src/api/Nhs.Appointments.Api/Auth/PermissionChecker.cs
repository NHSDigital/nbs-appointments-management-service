using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Auth;

public class PermissionChecker(IUserService userService, IRolesService rolesService, IMemoryCache cache)
    : IPermissionChecker
{
    public async Task<bool> HasPermissionAsync(string userId, IEnumerable<string> siteIds, string requiredPermission)
    {
        var globalPermissions = await GetFilteredPermissionsAsync(userId, ra => ra.Scope == "global");
        if (globalPermissions.Contains(requiredPermission))
        {
            return true;
        }

        foreach (var siteId in siteIds)
        {
            var usersPermissions = await GetFilteredPermissionsAsync(userId, ra => ra.Scope == $"site:{siteId}");
            if (usersPermissions.Contains(requiredPermission) == false)
            {
                return false;
            }
        }

        return siteIds.Any();
    }

    public async Task<bool> HasGlobalPermissionAsync(string userId, string requiredPermission)
    {
        var globalPermissions = await GetFilteredPermissionsAsync(userId, ra => ra.Scope == "global");
        return globalPermissions.Contains(requiredPermission);
    }

    public async Task<IEnumerable<string>> GetSitesWithPermissionAsync(string userId, string requiredPermission)
    {
        var siteIds = new List<string>();
        
        var userRoleAssignments = await GetUserRoleAssignmentsAsync(userId);

        var distinctSiteRoles = userRoleAssignments.Where(ra => ra.Scope.StartsWith("site:")).Distinct();

        foreach (var siteRole in distinctSiteRoles)
        {
            var usersPermissions = await GetFilteredPermissionsAsync(userId, ra => ra.Scope == siteRole.Scope && ra.Role == siteRole.Role);

            if (usersPermissions.Contains(requiredPermission))
            {
                siteIds.Add(siteRole.Scope.Replace("site:", ""));
            }
        }

        return siteIds.Distinct();
    }

    public Task<IEnumerable<string>> GetPermissionsAsync(string userId, string siteId)
    {
        Func<RoleAssignment, bool> filter = string.IsNullOrEmpty(siteId)
            ? ra => ra.Scope == "global"
            : ra => ra.Scope == "global" || ra.Scope == $"site:{siteId}";
        return GetFilteredPermissionsAsync(userId, filter);
    }

    private async Task<IEnumerable<string>> GetFilteredPermissionsAsync(string userId,
        Func<RoleAssignment, bool> filter)
    {
        var rolesTask = GetRolesAsync();
        var userRoleAssignmentsTask = GetUserRoleAssignmentsAsync(userId);

        await Task.WhenAll(rolesTask, userRoleAssignmentsTask);

        var userRoles = userRoleAssignmentsTask.Result.Where(filter).Select(ra => ra.Role);
        return rolesTask.Result.Where(r => userRoles.Contains(r.Id)).SelectMany(r => r.Permissions).Distinct();
    }

    private async Task<IEnumerable<RoleAssignment>> GetUserRoleAssignmentsAsync(string userId)
    {
        var cacheKey = $"user_roles_{userId}";
        if (cache.TryGetValue(cacheKey, out List<RoleAssignment> cachedRoleAssignments))
        {
            return cachedRoleAssignments;
        }

        var userRoleAssignmentsOp = await TryPattern.TryAsync(() => userService.GetUserRoleAssignments(userId));
        if (userRoleAssignmentsOp.Completed == false)
        {
            return [];
        }

        var userRoleAssignments = userRoleAssignmentsOp.Result.ToList();
        cache.Set(cacheKey, userRoleAssignments, TimeSpan.FromSeconds(20));
        return userRoleAssignments;
    }

    private async Task<IEnumerable<Role>> GetRolesAsync()
    {
        if (cache.TryGetValue("roles", out List<Role> cachedRoles))
        {
            return cachedRoles;
        }

        var rolesOp = await TryPattern.TryAsync(rolesService.GetRoles);
        if (rolesOp.Completed == false)
        {
            return [];
        }

        var roles = rolesOp.Result.ToList();
        cache.Set("roles", roles, TimeSpan.FromMinutes(5));
        return roles;
    }
}
