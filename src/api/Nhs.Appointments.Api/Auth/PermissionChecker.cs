using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Auth;

public class PermissionChecker(IUserSiteAssignmentService userSiteAssignmentService) : IPermissionChecker
{
    public async Task<bool> HasPermissionAsync(string userId, string requiredPermission)
    {
        var administrator = new Role()
        {
            Id = "canned:admin",
            Name = "Administrator",
            Permissions = new List<string>
            {
                "book:cancel",
                "BLAH-2"
            }
        };

        var roles = new[] { administrator };
        
        var userAssignments = await userSiteAssignmentService.GetUserAssignedSites(userId);
        var userRoles = userAssignments.SingleOrDefault(ua => ua.Site == "__global__")?.Roles ?? Array.Empty<string>();

        var usersPermissions = roles.Where(r => userRoles.Contains(r.Id)).SelectMany(r => r.Permissions).Distinct();
        return usersPermissions.Contains(requiredPermission);
    }
}
