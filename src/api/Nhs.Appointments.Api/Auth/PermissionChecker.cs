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
    private const string GlobalScope = "global";
    private const string RegionScopePrefix = "region:";
    private const string SiteScopePrefix = "site:";

    public async Task<bool> HasPermissionAsync(string userId, IEnumerable<string> siteIds, string requiredPermission)
    {
        var (roleAssignments, roles) = await GetUserPermissionsAsync(userId);

        var globalPermissions = FilteredPermissions(roleAssignments, roles, ra => ra.Scope == GlobalScope);
        if (globalPermissions.Contains(requiredPermission))
        {
            return true;
        }

        var regionalPermissions = roleAssignments
            .Where(ra => ra.Scope.StartsWith(RegionScopePrefix))
            .Select(ra => ra.Scope)
            .Distinct()
            .ToList();

        if (regionalPermissions.Count > 0)
        {
            var sitesInRegion = await GetSitesInRegions(regionalPermissions);
            if (sitesInRegion.Any(s => siteIds.Contains(s.Id)))
            {
                var sitePermissions = FilteredPermissions(roleAssignments, roles, ra => regionalPermissions.Contains(ra.Scope));
                if (sitePermissions.Contains(requiredPermission))
                {
                    return true;
                }
            }
        }

        foreach (var siteId in siteIds)
        {
            var usersPermissions = FilteredPermissions(roleAssignments, roles, ra => ra.Scope == $"{SiteScopePrefix}{siteId}");
            if (!usersPermissions.Contains(requiredPermission))
            {
                return false;
            }
        }

        return siteIds.Any();
    }

    public async Task<bool> HasGlobalPermissionAsync(string userId, string requiredPermission)
    {
        var (roleAssignments, roles) = await GetUserPermissionsAsync(userId);
        var globalPermissions = FilteredPermissions(roleAssignments, roles, ra => ra.Scope == GlobalScope);
        return globalPermissions.Contains(requiredPermission);
    }

    public async Task<IEnumerable<string>> GetPermissionsAsync(string userId, string siteId)
    {
        var filter = GlobalOrSiteFilter(siteId);

        if (string.IsNullOrEmpty(siteId))
        {
            filter = GlobalOnlyFilter();
        }

        var regionalPermissions = await GetRegionPermissionsAsync(userId);
        if (regionalPermissions.Any())
        {
            var sites = await GetSitesInRegions(regionalPermissions);
            if (sites.Any(s => s.Id == siteId))
            {
                filter = GlobalOrRegionalOrSiteFilter(regionalPermissions, siteId);
            }
        }

        var (roleAssignments, roles) = await GetUserPermissionsAsync(userId);

        return FilteredPermissions(roleAssignments, roles, filter);
    }

    public async Task<IEnumerable<string>> GetRegionPermissionsAsync(string userId)
    {
        var userRoleAssignments = await GetUserRoleAssignmentsAsync(userId);
        return userRoleAssignments
            .Where(ra => ra.Scope.StartsWith(RegionScopePrefix))
            .Select(ra => ra.Scope)
            .Distinct();
    }
    
    public async Task<IEnumerable<Site>> GetSitesWithPermissionAsync(string userId, string requiredPermission)
    {
        var scopes = await GetScopesWithPermissionsAsync(userId, requiredPermission);
        var sites = await siteService.GetAllSites();

        return sites.Where(site => ScopesAssignedToSite(scopes, site));
    }

    private bool ScopesAssignedToSite(IEnumerable<(string Scope, string Value)> scopes, Site site)
    {
        return scopes.Any(scope => ScopeAssignedToSite(scope, site));
    }

    private bool ScopeAssignedToSite((string Scope, string Value) scope, Site site)
    {
        return scope.Scope switch
        {
            GlobalScope => true,
            RegionScopePrefix => site.Region.Equals(scope.Value),
            SiteScopePrefix => site.Id.Equals(scope.Value),
            _ => false
        };
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

    private async Task<IEnumerable<(string Scope, string Value)>> GetScopesWithPermissionsAsync(string userId, string permission)
    {
        var (roleAssignments, roles) = await GetUserPermissionsAsync(userId);
        var rolesWithPermission = roles.Where(x => x.Permissions.Contains(permission)).Select(x => x.Id);
        
        return roleAssignments.Where(x => rolesWithPermission.Contains(x.Role))
            .Select(x => x.Scope.Split(':'))
            .Select(x => (Scope: x[0], Value: x[1]));
    }
    
    private async Task<(IEnumerable<RoleAssignment> roleAssignments, IEnumerable<Role> roles)> GetUserPermissionsAsync(string userId)
    {
        var roleAssignmentsTask = GetUserRoleAssignmentsAsync(userId);
        var rolesTask = GetRolesAsync();

        await Task.WhenAll(roleAssignmentsTask, rolesTask);

        return (roleAssignmentsTask.Result, rolesTask.Result);
    }

    private static IEnumerable<string> FilteredPermissions(
        IEnumerable<RoleAssignment> roleAssignments,
        IEnumerable<Role> roles,
        Func<RoleAssignment, bool> filter)
    {
        var userRoles = roleAssignments.Where(filter).Select(ra => ra.Role);
        return roles
            .Where(r => userRoles.Contains(r.Id))
            .SelectMany(r => r.Permissions)
            .Distinct();
    }

    private async Task<IEnumerable<Site>> GetSitesInRegions(IEnumerable<string> regionalPermissions)
    {
        var regionNames = regionalPermissions
            .Select(rp => rp.Replace(RegionScopePrefix, ""))
            .Distinct();

        var siteTasks = regionNames.Select(siteService.GetSitesInRegion);
        var siteResults = await Task.WhenAll(siteTasks);

        return siteResults.SelectMany(s => s).Distinct();
    }

    private static Func<RoleAssignment, bool> GlobalOnlyFilter()
        => ra => ra.Scope == GlobalScope;

    private static Func<RoleAssignment, bool> GlobalOrSiteFilter(string siteId)
        => ra => ra.Scope == GlobalScope || ra.Scope == $"{SiteScopePrefix}{siteId}";

    private static Func<RoleAssignment, bool> GlobalOrRegionalOrSiteFilter(IEnumerable<string> regionalPermissions, string siteId)
        => ra => ra.Scope == GlobalScope || ra.Scope == $"{SiteScopePrefix}{siteId}" || regionalPermissions.Contains(ra.Scope);
}
