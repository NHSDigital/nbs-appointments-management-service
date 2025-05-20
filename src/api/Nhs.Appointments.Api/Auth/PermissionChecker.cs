using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Auth;

public class PermissionChecker(IUserService userService, IRolesService rolesService, IMemoryCache cache, ISiteService siteService)
    : IPermissionChecker
{
    public async Task<bool> HasPermissionAsync(string userId, IEnumerable<string> siteIds, string requiredPermission)
    {
        var globalPermissions = await GetFilteredPermissionsAsync(userId, ra => ra.Scope == "global");
        if (globalPermissions.Contains(requiredPermission))
        {
            return true;
        }

        var regionalPermissions = await GetRegionPermissionsAsync(userId);
        if (regionalPermissions.Any())
        {
            var sitesInRegion = await GetSitesInRegions(regionalPermissions);
            if (sitesInRegion.Any(s => siteIds.Contains(s.Id)))
            {
                var sitePermissions = await GetFilteredPermissionsAsync(userId, ra => regionalPermissions.Contains(ra.Scope));
                if (sitePermissions.Contains(requiredPermission))
                {
                    return true;
                }
            }
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

    public async Task<IEnumerable<string>> GetPermissionsAsync(string userId, string siteId)
    {
        Func<RoleAssignment, bool> filter = ra => ra.Scope == "global" || ra.Scope == $"site:{siteId}";

        if (string.IsNullOrEmpty(siteId))
        {
            filter = ra => ra.Scope == "global";
        }

        var regionalPermissions = await GetRegionPermissionsAsync(userId);
        if (regionalPermissions.Any())
        {
            var sites = await GetSitesInRegions(regionalPermissions);
            if (sites.Any(s => s.Id == siteId))
            {
                filter = ra => regionalPermissions.Contains(ra.Scope);
            }
        }

        return await GetFilteredPermissionsAsync(userId, filter);
    }

    public async Task<IEnumerable<string>> GetRegionPermissionsAsync(string userId)
    {
        var userRoleAssignments = await GetUserRoleAssignmentsAsync(userId);
        return userRoleAssignments
            .Where(ra => ra.Scope.StartsWith("region:"))
            .Select(ra => ra.Scope)
            .Distinct();
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

    private async Task<IEnumerable<Site>> GetSitesInRegions(IEnumerable<string> regionalPermissions)
    {
        var sites = new List<Site>();

        foreach (var region in regionalPermissions.Select(rp => rp.Replace("region:", "")))
        {
            sites.AddRange(await siteService.GetSitesInRegion(region));
        }

        return sites.Distinct();
    }
}
