using System.Collections.Generic;
using System.Threading.Tasks;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Auth;

public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(string userId, IEnumerable<string> siteIds, string requiredPermission);
    
    /// <summary>
    /// Checks if the user has the required permission globally (i.e for all sites)
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="requiredPermission"></param>
    /// <returns></returns>
    Task<bool> HasGlobalPermissionAsync(string userId, string requiredPermission);
    
    Task<IEnumerable<string>> GetPermissionsAsync(string userId, string siteId);

    Task<IEnumerable<string>> GetRegionPermissionsAsync(string userId);
    
    /// <summary>
    /// Returns all sites that the user has the permission for.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="requiredPermission"></param>
    /// <returns>Site list</returns>
    Task<IEnumerable<Site>> GetSitesWithPermissionAsync(string userId, string requiredPermission);
}
